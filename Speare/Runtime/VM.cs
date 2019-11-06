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
            OpPush();
        }

        private byte[] ROM;
        private byte[] RAM;

        private Stack<byte[]> _scopes = new Stack<byte[]>();
        private Stack<byte[]> _pool = new Stack<byte[]>();
        private Dictionary<int, object> _globals = new Dictionary<int, object>();

        private IEnumerator _coroutine;

        private int _opAddress;
        private int _maxOpAddress;

        private int _mthAddress;
        private int _chrhAddress;
        private int _chrbAddress;

        private int _address = 0;
        public int AbsoluteAddress { get => _address; }
        public int RelativeAddress
        {
            get => _address - _opAddress;
            private set => _address = value + _opAddress;
        }

        private TimeSpan _frameBudget = TimeSpan.MaxValue;
        public TimeSpan FrameBudget { get => _frameBudget; set => _frameBudget = value; }

        public bool IsRunning { get; private set; }

        public int MemoryAllocated
        {
            get { return _pool.Count * Constants.SizeOfScope + _scopes.Count * Constants.SizeOfScope + ROM.Length; }
        }

        public object this[string varName]
        {
            get { return this[Hash.GetHashCode32(varName)]; }
            set { this[Hash.GetHashCode32(varName)] = value; }
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

        public void Load(byte[] byteCode)
        {
            ROM = byteCode;

            fixed (byte* rom = ROM)
            {
                _opAddress      = *(int*)(rom);
                _mthAddress     = *(int*)(rom + 4);
                _chrhAddress    = *(int*)(rom + 8);
                _chrbAddress    = *(int*)(rom + 12);

                // Max executable address = Method header start
                _maxOpAddress = _mthAddress;
            }
        }

        public void Allocate(int poolSize = 128)
        {
            for (int i = 0; i < poolSize; i++)
            {
                _pool.Push(new byte[Constants.SizeOfScope]);
            }
        }

        public void Jump(int address)
        {
            RelativeAddress = address;
        }

        public string ReadString(int stringIndex)
        {
            fixed (byte* rom = ROM)
            {
                int startIndex = *P.StringStartIndex(rom + _chrhAddress, stringIndex);
                int length = *P.StringLength(rom + _chrhAddress, stringIndex);

                return new string((sbyte*)(rom + _chrbAddress), startIndex, length, Encoding.Default);
            }
        }
        
        public object ReadRegister(Register register)
        {
            fixed (byte* ram = RAM)
            {
                switch (*P.DataType(ram, register))
                {
                    case DataType.Bool:
                        return *P.BoolValue(ram, register);
                    case DataType.Int:
                        return *P.IntValue(ram, register);
                    case DataType.Float:
                        return *P.FloatValue(ram, register);
                    case DataType.String:
                        return ReadString(*P.IntValue(ram, register));
                    case DataType.StringRef:
                        return _globals[*P.IntValue(ram, register)];
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
            fixed (byte* rom = ROM)
            {
                RelativeAddress = *P.MethodAddress(rom + _mthAddress, methodIndex);
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
            while (_address > _opAddress && _address < _maxOpAddress)
            {
                var op = MoveNext();

                switch (op)
                {
                    case Op.Push:
                        OpPush();
                        break;
                    case Op.Pop:
                        OpPop();
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
                else if (timer.Elapsed >= _frameBudget && Implementation.CoroutineRuntime != null)
                {
                    yield return Implementation.CoroutineRuntime.WaitForEndOfFrame();
                    timer.Restart();
                }
            }

            IsRunning = false;
        }

        private unsafe Op MoveNext()
        {
            fixed (byte* rom = ROM)
            {
                var result = *(Op*)(rom + _address);
                _address += 2;

                return result;
            }
        }

        private void OpPush()
        {
            if (_pool.Count > 0)
            {
                _scopes.Push(RAM = _pool.Pop());
                return;
            }

            _scopes.Push(RAM = new byte[34 * 5]);
        }

        private void OpPop()
        {
            _pool.Push(_scopes.Pop());
            RAM = _scopes.Peek();
        }

        private void OpConstant()
        {
            fixed (byte* rom = ROM)
            fixed (byte* ram = RAM)
            {
                var register = *(Register*)(rom + _address);

                *P.DataType(ram, register) = *(DataType*)(rom + _address + 1);
                *P.IntValue(ram, register) = *(int*)(rom + _address + 2);
                
                _address += 6;
            }
        }

        private void OpGlobalRead()
        {
            fixed (byte* rom = ROM)
            fixed (byte* ram = RAM)
            {
                var register = *(Register*)(rom + _address);
                var hash = *(int*)(rom + _address + 1);
                _address += 5;

                var value = this[hash];
                if (value == null)
                {
                    *P.DataType(ram, register) = DataType.Null;
                    return;
                }

                var type = value.GetType();
                if (type == typeof(int))
                {
                    *P.DataType(ram, register) = DataType.Int;
                    *P.IntValue(ram, register) = (int)value;
                }
                if (type == typeof(float))
                {
                    *P.DataType(ram, register) = DataType.Float;
                    *P.FloatValue(ram, register) = (float)value;
                }
                if (type == typeof(bool))
                {
                    *P.DataType(ram, register) = DataType.Bool;
                    *P.BoolValue(ram, register) = (bool)value;
                }
                if (type == typeof(string))
                {
                    *P.DataType(ram, register) = DataType.StringRef;
                    *P.IntValue(ram, register) = hash;
                }
            }
        }

        private void OpGlobalWrite()
        {
            fixed (byte* rom = ROM)
            {
                var hash = *(int*)(rom + _address);
                var register = *(Register*)(rom + _address + 4);

                _address += 5;

                this[hash] = ReadRegister(register);
            }
        }

        private void OpSet()
        {
            fixed (byte* rom = ROM)
            fixed (byte* ram = RAM)
            {
                var destination = *(Register*)(rom + _address);
                var source = *(Register*)(rom + _address + 1);

                _address += 2;

                *P.DataType(ram, destination) = *P.DataType(ram, source);
                *P.IntValue(ram, destination) = *P.IntValue(ram, source);
            }
        }

        private void OpJump()
        {
            fixed (byte* rom = ROM)
            {
                RelativeAddress = *(int*)(rom + _address);
            }
        }

        private void OpJumpIf()
        {
            fixed (byte* rom = ROM)
            fixed (byte* ram = RAM)
            {
                if (*P.DataType(ram, Register.LastResult) != DataType.Bool ||
                    *P.BoolValue(ram, Register.LastResult) == false)
                {
                    _address += 4;
                    return;
                }

                RelativeAddress = *(int*)(rom + _address);
            }
        }

        private void OpCall()
        {
            fixed (byte* rom = ROM)
            {
                var methodIndex = *(short*)(rom + _address);
                var parameterCount = *P.MethodParameterCount(rom + _mthAddress, methodIndex);

                var currentRam = RAM;
                OpPush();

                fixed (byte* ram = RAM)
                fixed (byte* previousRam = currentRam)
                {
                    *P.DataType(ram, Register.ReturnAddress) = DataType.Int;
                    *P.IntValue(ram, Register.ReturnAddress) = _address;
                    
                    // TODO: Instead of copying the registers from the current ram the compiler
                    //       should be responsible of creating a new ram and running the passed
                    //       parameter OPs

                    for (int i = 0; i < parameterCount; i++)
                    {
                        *P.DataType(ram, Register.Param0, i) = *P.DataType(previousRam, Register.Param0, i);
                        *P.IntValue(ram, Register.Param0, i) = *P.IntValue(previousRam, Register.Param0, i);
                    }

                    RelativeAddress = *P.MethodAddress(rom + _mthAddress, methodIndex);
                }
            }
        }

        private void OpReturn()
        {
            fixed (byte* previous = RAM)
            {
                OpPop();

                fixed (byte* ram = RAM)
                {
                    // Copy last result
                    *P.DataType(ram, Register.LastResult) = *P.DataType(previous, Register.LastResult);
                    *P.IntValue(ram, Register.LastResult) = *P.IntValue(previous, Register.LastResult);
                }

                _address = *P.IntValue(previous, Register.ReturnAddress);
            }
        }

        private void OpInterop()
        {
            fixed (byte* rom = ROM)
            {
                var hash = *(int*)(rom + _address);

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
            fixed (byte* rom = ROM)
            fixed (byte* ram = RAM)
            {
                var a = *(Register*)(rom + _address);
                var b = *(Register*)(rom + _address + 1);
                var arithmetic = *(Arithmetic*)(rom + _address + 2);

                _address += 3;

                var function = Arithmetics.Get(
                    *P.DataType(ram, a),
                    *P.DataType(ram, b),
                    arithmetic
                );

                if (function == null)
                    return;

                function(ram, a, b);
            }
        }

        private void OpExit()
        {
            _address = _maxOpAddress;
        }

        private void OpDebugPrint()
        {
            fixed (byte* rom = ROM)
            {
                var register = *(Register*)(rom + _address);
                _address++;

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
