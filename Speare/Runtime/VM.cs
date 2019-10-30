using Speare.Compiler;
using Speare.Utility;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtime
{
    public unsafe class VM
    {
        public VM()
        {
            OpPushScope();
        }

        public static IGameRuntime GameRuntime;
        public static ICoroutineRuntime CoroutineRuntime;

        private byte[] _byteCode;

        private Stack<byte[]> _stack = new Stack<byte[]>();
        private Stack<byte[]> _scopePool = new Stack<byte[]>();
        private Dictionary<int, object> _globals = new Dictionary<int, object>();

        private IEnumerator _coroutine;

        private int _opAddress;
        private int _maxOpAddress;

        private int _mthAddress;
        private int _chrhAddress;
        private int _chrbAddress;

        private int _byteAddress;
        public int ByteAddress { get => _byteAddress; set => _byteAddress = value; }
        public int Address { get => _byteAddress - _opAddress; set => _byteAddress = value + _opAddress; }

        private TimeSpan _frameBudget;
        public TimeSpan FrameBudget { get => _frameBudget; set => _frameBudget = value; }

        public bool IsRunning { get; private set; }

        public void Load(byte[] byteCode)
        {
            _byteCode = byteCode;

            fixed (byte* pointer = byteCode)
            {
                _opAddress      = *(int*)(pointer);
                _mthAddress     = *(int*)(pointer + 4);
                _chrhAddress    = *(int*)(pointer + 8);
                _chrbAddress    = *(int*)(pointer + 12);

                // Max executable address = Method header start
                _maxOpAddress = _mthAddress;
            }
        }

        public object this[string varName]
        {
            get { return this[varName.GetHashCode32()]; }
            set { this[varName.GetHashCode32()] = value; }
        }

        public object this[int varHash]
        {
            get
            {
                object result;
                _globals.TryGetValue(varHash, out result);
                return result;
            }
            set
            {
                _globals[varHash] = value;
            }
        }

        public byte[] Scope
        {
            get { return _stack.Peek(); }
        }

        public int MemoryAllocated
        {
            get { return _scopePool.Count * Constants.SizeOfScope + _stack.Count * Constants.SizeOfScope + _byteCode.Length; }
        }

        public void Allocate(int poolSize = 128)
        {
            for (int i = 0; i < poolSize; i++)
            {
                _scopePool.Push(new byte[Constants.SizeOfScope]);
            }
        }

        public void Jump(int address)
        {
            Address = address;
        }

        public string ReadString(int stringIndex)
        {
            fixed (byte* pointer = _byteCode)
            {
                int startIndex = *P.StringStartIndex(pointer + _chrhAddress, stringIndex);
                int length = *P.StringLength(pointer + _chrhAddress, stringIndex);

                return new string((sbyte*)(pointer + _chrbAddress), startIndex, length, Encoding.Default);
            }
        }
        
        public object ReadRegister(Register register)
        {
            fixed (byte* scope = Scope)
            {
                switch (*P.DataType(scope, register))
                {
                    case DataType.Bool:
                        return *P.BoolValue(scope, register);
                    case DataType.Int:
                        return *P.IntValue(scope, register);
                    case DataType.Float:
                        return *P.FloatValue(scope, register);
                    case DataType.String:
                        return ReadString(*P.IntValue(scope, register));
                    case DataType.StringRef:
                        return _globals[*P.IntValue(scope, register)];
                    default:
                        return null;
                }
            }
        }

        public void Run(int methodIndex)
        {
            var coroutine = RunCoroutine(methodIndex);
            while (coroutine.MoveNext() && IsRunning);
        }

        public IEnumerator RunCoroutine(int methodIndex)
        {
            fixed (byte* pointer = _byteCode)
            {
                Address = *P.MethodAddress(pointer + _mthAddress, methodIndex);
            }
            return RunCoroutine();
        }

        public void Run()
        {
            var coroutine = RunCoroutine();
            while (coroutine.MoveNext() && IsRunning);
        }

        public IEnumerator RunCoroutine()
        {
            IsRunning = true;

            var timer = Stopwatch.StartNew();
            while (_byteAddress > _opAddress && _byteAddress < _maxOpAddress)
            {
                var op = MoveNext();

                switch (op)
                {
                    case Op.PushScope:
                        OpPushScope();
                        break;
                    case Op.PopScope:
                        OpPopScope();
                        break;
                    case Op.Constant:
                        OpConstant();
                        break;
                    case Op.GlobalRead:
                        OpGlobalRead();
                        break;
                    case Op.GlobalWrite:
                        OpGlobalWrite();
                        break;
                    case Op.Set:
                        OpSet();
                        break;
                    case Op.Call:
                        OpCall();
                        break;
                    case Op.Return:
                        OpReturn();
                        break;
                    case Op.Interop:
                        OpInterop();
                        break;
                    case Op.Jump:
                        OpJump();
                        break;
                    case Op.JumpIf:
                        OpJumpIf();
                        break;
                    case Op.Arithmetic:
                        OpArithmetic();
                        break;
                    case Op.MethodDefinition:
                    case Op.Exit:
                        OpExit();
                        break;
                    case Op.DebugPrint:
                        OpDebugPrint();
                        break;
                }

                if (_coroutine != null)
                {
                    yield return _coroutine;
                    _coroutine = null;
                }
                else if (timer.Elapsed >= _frameBudget && CoroutineRuntime != null)
                {
                    yield return CoroutineRuntime.WaitForEndOfFrame();
                    timer.Restart();
                }
            }

            IsRunning = false;
        }

        private unsafe Op MoveNext()
        {
            fixed (byte* pointer = _byteCode)
            {
                var result = *(Op*)(pointer + _byteAddress);
                _byteAddress += 2;

                return result;
            }
        }

        private void OpPushScope()
        {
            if (_scopePool.Count > 0)
            {
                _stack.Push(_scopePool.Pop());
                return;
            }

            _stack.Push(new byte[34 * 5]);
        }

        private void OpPopScope()
        {
            _scopePool.Push(_stack.Pop());
        }

        private void OpConstant()
        {
            fixed (byte* pointer = _byteCode)
            fixed (byte* scope = Scope)
            {
                var register = *(Register*)(pointer + _byteAddress);

                *P.DataType(scope, register) = *(DataType*)(pointer + _byteAddress + 1);
                *P.IntValue(scope, register) = *(int*)(pointer + _byteAddress + 2);
                
                _byteAddress += 6;
            }
        }

        private void OpGlobalRead()
        {
            fixed (byte* pointer = _byteCode)
            fixed (byte* scope = Scope)
            {
                var register = *(Register*)(pointer + _byteAddress);
                var hash = *(int*)(pointer + _byteAddress + 1);
                _byteAddress += 5;

                var value = this[hash];
                if (value == null)
                {
                    *P.DataType(scope, register) = DataType.Null;
                    return;
                }

                var type = value.GetType();
                if (type == typeof(int))
                {
                    *P.DataType(scope, register) = DataType.Int;
                    *P.IntValue(scope, register) = (int)value;
                }
                if (type == typeof(float))
                {
                    *P.DataType(scope, register) = DataType.Float;
                    *P.FloatValue(scope, register) = (float)value;
                }
                if (type == typeof(bool))
                {
                    *P.DataType(scope, register) = DataType.Bool;
                    *P.BoolValue(scope, register) = (bool)value;
                }
                if (type == typeof(string))
                {
                    *P.DataType(scope, register) = DataType.StringRef;
                    *P.IntValue(scope, register) = hash;
                }
            }
        }

        private void OpGlobalWrite()
        {
            fixed (byte* pointer = _byteCode)
            {
                var hash = *(int*)(pointer + _byteAddress);
                var register = *(Register*)(pointer + _byteAddress + 4);

                _byteAddress += 5;

                this[hash] = ReadRegister(register);
            }
        }

        private void OpSet()
        {
            fixed (byte* pointer = _byteCode)
            fixed (byte* scope = Scope)
            {
                var destination = *(Register*)(pointer + _byteAddress);
                var source = *(Register*)(pointer + _byteAddress + 1);

                _byteAddress += 2;

                *P.DataType(scope, destination) = *P.DataType(scope, source);
                *P.IntValue(scope, destination) = *P.IntValue(scope, source);
            }
        }

        private void OpJump()
        {
            fixed (byte* pointer = _byteCode)
            {
                Address = *(int*)(pointer + _byteAddress);
            }
        }

        private void OpJumpIf()
        {
            fixed (byte* pointer = _byteCode)
            fixed (byte* scope = Scope)
            {
                if (*P.DataType(scope, Register.LastResult) != DataType.Bool ||
                    *P.BoolValue(scope, Register.LastResult) == false)
                {
                    _byteAddress += 4;
                    return;
                }

                Address = *(int*)(pointer + _byteAddress);
            }
        }

        private void OpCall()
        {
            fixed (byte* pointer = _byteCode)
            {
                var methodIndex = *(short*)(pointer + _byteAddress);
                var parameterCount = *P.MethodParameterCount(pointer + _mthAddress, methodIndex);

                var previous = Scope;
                OpPushScope();

                fixed (byte* scope = Scope)
                fixed (byte* previosScope = previous)
                {
                    *P.DataType(scope, Register.ReturnAddress) = DataType.Int;
                    *P.IntValue(scope, Register.ReturnAddress) = _byteAddress;
                    
                    // TODO: Instead of copying the registers from the current scope the compiler
                    //       should be responsible of creating a new scope and running the passed
                    //       parameter OPs

                    for (int i = 0; i < parameterCount; i++)
                    {
                        *P.DataType(scope, Register.Param0, i) = *P.DataType(previosScope, Register.Param0, i);
                        *P.IntValue(scope, Register.Param0, i) = *P.IntValue(previosScope, Register.Param0, i);
                    }

                    Address = *P.MethodAddress(pointer + _mthAddress, methodIndex);
                }
            }
        }

        private void OpReturn()
        {
            fixed (byte* previous = Scope)
            {
                OpPopScope();

                fixed (byte* scope = Scope)
                {
                    // Copy last result
                    *P.DataType(scope, Register.LastResult) = *P.DataType(previous, Register.LastResult);
                    *P.IntValue(scope, Register.LastResult) = *P.IntValue(previous, Register.LastResult);
                }

                _byteAddress = *P.IntValue(previous, Register.ReturnAddress);
            }
        }

        private void OpInterop()
        {
            fixed (byte* pointer = _byteCode)
            {
                var hash = *(int*)(pointer + _byteAddress);

                var info = Interop.Methods[hash];
                var parameters = Interop.ParameterPool[hash];
                var offset = (byte)Register.Param0;

                for (byte i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = ReadRegister((Register)(i + offset));
                }

                var coroutine = info.Invoke(null, parameters) as IEnumerator;
                if (coroutine == null)
                    return;

                _coroutine = coroutine;
            }
        }

        private void OpArithmetic()
        {
            fixed (byte* pointer = _byteCode)
            fixed (byte* scope = Scope)
            {
                var a = *(Register*)(pointer + _byteAddress);
                var b = *(Register*)(pointer + _byteAddress + 1);
                var arithmetic = *(Arithmetic*)(pointer + _byteAddress + 2);

                _byteAddress += 3;

                var function = Arithmetics.Get(
                    *P.DataType(scope, a),
                    *P.DataType(scope, b),
                    arithmetic
                );

                if (function == null)
                    return;

                function(scope, a, b);
            }
        }

        private void OpExit()
        {
            _byteAddress = _maxOpAddress;
        }

        private void OpDebugPrint()
        {
            fixed (byte* pointer = _byteCode)
            {
                var register = *(Register*)(pointer + _byteAddress);
                _byteAddress++;

                Console.WriteLine(ReadRegister(register));
            }
        }

        public static VM FromBuilder(OpBuilder ops)
        {
            return FromByteCode(ops.Build());
        }

        public static VM FromByteCode(byte[] code)
        {
            var vm = new VM();
            vm.Load(code);
            return vm;
        }
    }
}
