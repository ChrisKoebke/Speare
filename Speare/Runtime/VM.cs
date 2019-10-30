using Speare.Compilation;
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

        public Stack<byte[]> Stack = new Stack<byte[]>();
        public Stack<byte[]> ScopePool = new Stack<byte[]>();

        public Dictionary<string, object> Globals = new Dictionary<string, object>();

        public byte[] Ops;
        public byte[] Chrh;
        public byte[] Chrb;
        public byte[] Mth;

        public int Address = 0;
        public IEnumerator Coroutine;

        public byte[] Scope
        {
            get { return Stack.Peek(); }
        }

        public int MemoryAllocated
        {
            get { return ScopePool.Count * 130 + Stack.Count * 130 + Ops.Length + Chrh.Length + Chrb.Length + Mth.Length; }
        }

        public void Allocate(int poolSize = 128)
        {
            for (int i = 0; i < poolSize; i++)
            {
                ScopePool.Push(new byte[26 * 5]);
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

        public string ReadChrb(int stringIndex)
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
                    case DataType.ChrPointer:
                        return ReadChrb(*P.IntValue(scope, register));
                    default:
                        return null;
                }
            }
        }

        public void OpPushScope()
        {
            if (ScopePool.Count > 0)
            {
                Stack.Push(ScopePool.Pop());
                return;
            }

            Stack.Push(new byte[26 * 5]);
        }

        public void OpPopScope()
        {
            ScopePool.Push(Stack.Pop());
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

        public void OpMove()
        {
            fixed (byte* ops = Ops)
            fixed (byte* scope = Scope)
            {
                var source = *(Register*)(ops + Address);
                var destination = *(Register*)(ops + Address + 1);

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
                    case Op.Move:
                        OpMove();
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
