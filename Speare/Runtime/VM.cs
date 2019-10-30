using Speare.Compilation;
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

        public TimeSpan FrameBudget;

        private Stack<byte[]> _stack = new Stack<byte[]>();
        private Stack<byte[]> _scopePool = new Stack<byte[]>();

        private Dictionary<int, object> _globals = new Dictionary<int, object>();

        public byte[] Ops;
        public byte[] Chrh;
        public byte[] Chrb;
        public byte[] Mth;

        public int Address = 0;
        public IEnumerator Coroutine;

        public object this[int hash]
        {
            get
            {
                object result;
                _globals.TryGetValue(hash, out result);
                return result;
            }
            set
            {
                _globals[hash] = value;
            }
        }

        public object this[string name]
        {
            get { return this[name.GetHashCode32()]; }
            set { this[name.GetHashCode32()] = value; }
        }

        public byte[] Scope
        {
            get { return _stack.Peek(); }
        }

        public int MemoryAllocated
        {
            get { return _scopePool.Count * 170 + _stack.Count * 170 + Ops.Length + Chrh.Length + Chrb.Length + Mth.Length; }
        }

        public void Allocate(int poolSize = 128)
        {
            for (int i = 0; i < poolSize; i++)
            {
                _scopePool.Push(new byte[34 * 5]);
            }
        }

        public unsafe Op Next()
        {
            fixed (byte* pointer = Ops)
            {
                var result = *(Op*)(pointer + Address);
                Address += 2;

                return result;
            }
        }

        public string ReadChrbString(int stringIndex)
        {
            fixed (byte* chrh = Chrh)
            {
                int startIndex = *P.StringStartIndex(chrh, stringIndex);
                int length = *P.StringLength(chrh, stringIndex);

                fixed (byte* buffer = Chrb)
                {
                    return new string((sbyte*)buffer, startIndex, length, Encoding.Default);
                }
            }
        }
        
        public object ReadRegisterBoxed(Register register)
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
                        return ReadChrbString(*P.IntValue(scope, register));
                    case DataType.StringRef:
                        return _globals[*P.IntValue(scope, register)];
                    default:
                        return null;
                }
            }
        }

        public void OpPushScope()
        {
            if (_scopePool.Count > 0)
            {
                _stack.Push(_scopePool.Pop());
                return;
            }

            _stack.Push(new byte[34 * 5]);
        }

        public void OpPopScope()
        {
            _scopePool.Push(_stack.Pop());
        }

        public void OpConstant()
        {
            fixed (byte* ops = Ops)
            fixed (byte* scope = Scope)
            {
                var register = *(Register*)(ops + Address);

                *P.DataType(scope, register) = *(DataType*)(ops + Address + 1);
                *P.IntValue(scope, register) = *(int*)(ops + Address + 2);
                
                Address += 6;
            }
        }

        public void OpGlobalRead()
        {
            fixed (byte* ops = Ops)
            fixed (byte* scope = Scope)
            {
                var register = *(Register*)(ops + Address);
                var hash = *(int*)(ops + Address + 1);
                Address += 5;

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

        public void OpGlobalWrite()
        {
            fixed (byte* ops = Ops)
            fixed (byte* scope = Scope)
            {
                var hash = *(int*)(ops + Address);
                var register = *(Register*)(ops + Address + 4);

                Address += 5;

                this[hash] = ReadRegisterBoxed(register);
            }
        }

        public void OpSet()
        {
            fixed (byte* ops = Ops)
            fixed (byte* scope = Scope)
            {
                var destination = *(Register*)(ops + Address);
                var source = *(Register*)(ops + Address + 1);

                Address += 2;

                *P.DataType(scope, destination) = *P.DataType(scope, source);
                *P.IntValue(scope, destination) = *P.IntValue(scope, source);
            }
        }

        public void OpCompare()
        {
            fixed (byte* ops = Ops)
            fixed (byte* scope = Scope)
            {
                var a = *(Register*)(ops + Address);
                var b = *(Register*)(ops + Address + 1);

                Address += 2;

                *P.DataType(scope, Register.LastResult) = DataType.Bool;
                *P.BoolValue(scope, Register.LastResult) = *P.IntValue(scope, a) < *P.IntValue(scope, b);

                // TODO: Implement comparison (for now we just skip the comparison byte)
                Address += 1;
            }
        }

        public void OpJump()
        {
            fixed (byte* ops = Ops)
            {
                Address = *(int*)(ops + Address);
            }
        }

        public void OpJumpIf()
        {
            fixed (byte* ops = Ops)
            fixed (byte* scope = Scope)
            {
                if (*P.DataType(scope, Register.LastResult) != DataType.Bool ||
                    *P.BoolValue(scope, Register.LastResult) == false)
                {
                    Address += 4;
                    return;
                }

                Address = *(int*)(ops + Address);
            }
        }

        public void OpCall()
        {
            fixed (byte* ops = Ops)
            fixed (byte* mth = Mth)
            {
                var methodIndex = *(short*)(ops + Address);
                var parameterCount = *P.MethodParameterCount(mth, methodIndex);

                var previous = Scope;
                OpPushScope();

                fixed (byte* scope = Scope)
                fixed (byte* previosScope = previous)
                {
                    *P.DataType(scope, Register.ReturnAddress) = DataType.Int;
                    *P.IntValue(scope, Register.ReturnAddress) = Address;
                    
                    // TODO: Instead of copying the registers from the current scope the compiler
                    //       should be responsible of creating a new scope and running the passed
                    //       parameter OPs

                    for (int i = 0; i < parameterCount; i++)
                    {
                        *P.DataType(scope, Register.Param0, i) = *P.DataType(previosScope, Register.Param0, i);
                        *P.IntValue(scope, Register.Param0, i) = *P.IntValue(previosScope, Register.Param0, i);
                    }

                    Address = *P.MethodAddress(mth, methodIndex);
                }
            }
        }

        public void OpReturn()
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

                Address = *P.IntValue(previous, Register.ReturnAddress);
            }
        }

        public void OpInterop()
        {
            fixed (byte* pointer = Ops)
            {
                var hash = *(int*)(pointer + Address);

                var info = Interop.Methods[hash];
                var parameters = Interop.ParameterPool[hash];
                var offset = (byte)Register.Param0;

                for (byte i = 0; i < parameters.Length; i++)
                {
                    parameters[i] = ReadRegisterBoxed((Register)(i + offset));
                }

                var coroutine = info.Invoke(null, parameters) as IEnumerator;
                if (coroutine == null)
                    return;

                Coroutine = coroutine;
            }
        }

        public void OpAdd()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                var registerA = *(Register*)(pointer + Address);
                var registerB = *(Register*)(pointer + Address + 1);

                var typeA = *P.DataType(scope, registerA);
                var typeB = *P.DataType(scope, registerB);

                Address += 2;
                
                // TODO: Type table for faster operator look up

                if (typeA == DataType.Int && typeB == DataType.Int)
                {
                    *P.DataType(scope, Register.LastResult) = DataType.Int;
                    *P.IntValue(scope, Register.LastResult) = *P.IntValue(scope, registerA) + *P.IntValue(scope, registerB);
                }
                else if (typeA == DataType.Int && typeB == DataType.Float)
                {
                    *P.DataType(scope, Register.LastResult) = DataType.Float;
                    *P.FloatValue(scope, Register.LastResult) = *P.IntValue(scope, registerA) + *P.FloatValue(scope, registerB);
                }
                else if (typeA == DataType.Float && typeB == DataType.Int)
                {
                    *P.DataType(scope, Register.LastResult) = DataType.Float;
                    *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, registerA) + *P.IntValue(scope, registerB);
                }
                else if (typeA == DataType.Float && typeB == DataType.Float)
                {
                    *P.DataType(scope, Register.LastResult) = DataType.Float;
                    *P.FloatValue(scope, Register.LastResult) = *P.FloatValue(scope, registerA) + *P.FloatValue(scope, registerB);
                }
            }
        }

        public void OpExit()
        {
            Address = Ops.Length;
        }

        public void OpDebugPrint()
        {
            fixed (byte* pointer = Ops)
            {
                var register = *(Register*)(pointer + Address);
                Address++;

                Console.WriteLine(ReadRegisterBoxed(register));
            }
        }

        public IEnumerator Run(int methodIndex)
        {
            fixed (byte* mth = Mth)
            {
                Address = *P.MethodAddress(mth, methodIndex);
            }
            return Run();
        }

        public IEnumerator Run()
        {
            var timer = Stopwatch.StartNew();

            while (Address < Ops.Length)
            {
                var op = Next();

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
                    case Op.Compare:
                        OpCompare();
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
                    case Op.Add:
                        OpAdd();
                        break;
                    case Op.Subtract:
                        break;
                    case Op.Divide:
                        break;
                    case Op.Multiply:
                        break;
                    case Op.Modulo:
                        break;
                    case Op.Equal:
                        break;
                    case Op.Not:
                        break;
                    case Op.Method:
                    case Op.Exit:
                        OpExit();
                        break;
                    case Op.DebugPrint:
                        OpDebugPrint();
                        break;
                }

                if (Coroutine != null)
                {
                    yield return Coroutine;
                    Coroutine = null;
                }
                else if (timer.Elapsed >= FrameBudget && CoroutineRuntime != null)
                {
                    yield return CoroutineRuntime.WaitForEndOfFrame();
                    timer.Restart();
                }
            }
        }

        public static VM FromBuilder(OpBuilder ops)
        {
            var vm = new VM();
            ops.Build(out vm.Ops, out vm.Chrh, out vm.Chrb, out vm.Mth);
            return vm;
        }
    }
}
