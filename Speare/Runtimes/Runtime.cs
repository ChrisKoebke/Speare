using Speare.Ops;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtimes
{
    public unsafe class Runtime
    {
        public Runtime()
        {
            OpPushScope();
        }

        public static IRuntime Implementation;

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

        public int Size
        {
            get { return Ops.Length + Chrh.Length + Chrb.Length + Mth.Length; }
        }

        public static Runtime FromBuilder(OpBuilder ops)
        {
            var runtime = new Runtime();
            ops.Build(out runtime.Ops, out runtime.Chrh, out runtime.Chrb, out runtime.Mth);
            return runtime;
        }

        public unsafe OpCode ReadOp()
        {
            fixed (byte* pointer = Ops)
            {
                var result = *(OpCode*)(pointer + Address);
                Address += 2;

                return result;
            }
        }

        public string ReadChrb(int headerIndex)
        {
            fixed (byte* headerPointer = Chrh)
            {
                int startIndex = *(int*)(headerPointer + headerIndex * 8);
                int length = *(int*)(headerPointer + headerIndex * 8 + 4);

                fixed (byte* buffer = Chrb)
                {
                    return new string((sbyte*)buffer, startIndex, length, Encoding.Default);
                }
            }
        }

        public object ReadRegister(Register register)
        {
            return ReadRegister((byte)register);
        }

        public object ReadRegister(byte index)
        {
            fixed (byte* scope = Scope)
            {
                switch (*(DataType*)(scope + index * 5))
                {
                    case DataType.Bool:
                        return *(bool*)(scope + index * 5 + 1);
                    case DataType.Int:
                        return *(int*)(scope + index * 5 + 1);
                    case DataType.Float:
                        return *(float*)(scope + index * 5 + 1);
                    case DataType.ChrPointer:
                        return ReadChrb(*(int*)(scope + index * 5 + 1));
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

            Stack.Push(new byte[32 * 5]);
        }

        public void OpPopScope()
        {
            ScopePool.Push(Stack.Pop());
        }

        public void OpConstant()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                var register = *(pointer + Address);

                *(DataType*)(scope + register * 5) = *(DataType*)(pointer + Address + 1);
                *(int*)(scope + register * 5 + 1) = *(int*)(pointer + Address + 2);

                Address += 6;
            }
        }

        public void OpMove()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                var source = *(pointer + Address);
                var destination = *(pointer + Address + 1);

                Address += 2;

                *(DataType*)(scope + destination * 5) = *(DataType*)(scope + source * 5);
                *(int*)(scope + destination * 5 + 1) = *(int*)(scope + source * 5 + 1);
            }
        }

        public void OpCompare()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                var a = *(pointer + Address);
                var b = *(pointer + Address + 1);

                Address += 2;

                *(DataType*)(scope) = DataType.Bool;
                *(bool*)(scope + 1) = (int)ReadRegister(a) < (int)ReadRegister(b);

                Address += 1;
            }
        }

        public void OpJump()
        {
            fixed (byte* pointer = Ops)
            {
                Address = *(int*)(pointer + Address);
            }
        }

        public void OpJumpIfTrue()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                // Last result is false
                if (*(DataType*)scope != DataType.Bool || *(bool*)(scope + 1) == false)
                {
                    Address += 4;
                    return;
                }

                Address = *(int*)(pointer + Address);
            }
        }

        public void OpCall()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* mth = Mth)
            {
                var methodIndex = *(short*)(pointer + Address);
                var parameterCount = *(mth + methodIndex * 3 + 2);

                var previous = Scope;
                OpPushScope();

                fixed (byte* scope = Scope)
                fixed (byte* previosScope = previous)
                {
                    *(DataType*)(scope + (int)Register.ReturnAddress * 5) = DataType.Int;
                    *(int*)(scope + (int)Register.ReturnAddress * 5 + 1) = Address;

                    // TODO: Instead of copying the registers from the current scope the compiler
                    //       should be responsible of creating a new scope and running the passed
                    //       parameter OPs

                    for (int i = 0; i < parameterCount; i++)
                    {
                        *(DataType*)(scope + ((int)Register.A + i) * 5) = *(DataType*)(previosScope + ((int)Register.A + i) * 5);
                        *(int*)(scope + ((int)Register.A + i) * 5 + 1) = *(int*)(previosScope + ((int)Register.A + i) * 5 + 1);
                    }

                    Address = *(short*)(mth + methodIndex * 3);
                }
            }
        }

        public void OpReturn()
        {
            fixed (byte* previous = Scope)
            {
                var returnAddress = ReadRegister(Register.ReturnAddress);
                OpPopScope();

                fixed (byte* scope = Scope)
                {
                    // Copy last result
                    *(DataType*)scope = *(DataType*)previous;
                    *(int*)(scope + 1) = *(int*)(previous + 1);
                }

                Address = (int)returnAddress;
            }
        }

        public void OpInterop()
        {
            fixed (byte* pointer = Ops)
            {
                var hash = *(int*)(pointer + Address);

                var info = Interop.Methods[hash];
                var parameters = Interop.ParameterPool[hash];
                var offset = (byte)Register.A;

                for (byte i = 0; i <= parameters.Length; i++)
                {
                    parameters[i] = ReadRegister((byte)(i + offset));
                }

                info.Invoke(null, parameters);
            }
        }

        public void OpAdd()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                var registerA = *(pointer + Address);
                var typeA = *(DataType*)(scope + registerA * 5);

                var registerB = *(pointer + Address + 1);
                var typeB = *(DataType*)(scope + registerB * 5);

                Address += 2;
                
                if (typeA == DataType.Int && typeB == DataType.Int)
                {
                    *(DataType*)(scope) = DataType.Int;
                    *(int*)(scope + 1) = *(int*)(scope + registerA * 5 + 1) + *(int*)(scope + registerB * 5 + 1);
                }
                else if (typeA == DataType.Int && typeB == DataType.Float)
                {
                    *(DataType*)(scope) = DataType.Float;
                    *(float*)(scope + 1) = *(int*)(scope + registerA * 5 + 1) + *(float*)(scope + registerB * 5 + 1);
                }
                else if (typeA == DataType.Float && typeB == DataType.Int)
                {
                    *(DataType*)(scope) = DataType.Float;
                    *(float*)(scope + 1) = *(float*)(scope + registerA * 5 + 1) + *(int*)(scope + registerB * 5 + 1);
                }
                else if (typeA == DataType.Float && typeB == DataType.Float)
                {
                    *(DataType*)(scope) = DataType.Float;
                    *(float*)(scope + 1) = *(float*)(scope + registerA * 5 + 1) + *(float*)(scope + registerB * 5 + 1);
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
                var register = *(pointer + Address);
                Address++;

                Console.WriteLine(ReadRegister(register));
            }
        }

        public IEnumerator Run(int methodIndex)
        {
            fixed (byte* mth = Mth)
            {
                Address = *(short*)(mth + methodIndex * 3);
            }
            return Run();
        }

        public IEnumerator Run()
        { 
            while (Address < Ops.Length)
            {
                var op = ReadOp();

                switch (op)
                {
                    case OpCode.PushScope:
                        OpPushScope();
                        break;
                    case OpCode.PopScope:
                        OpPopScope();
                        break;
                    case OpCode.Constant:
                        OpConstant();
                        break;
                    case OpCode.Move:
                        OpMove();
                        break;
                    case OpCode.Compare:
                        OpCompare();
                        break;
                    case OpCode.Call:
                        OpCall();
                        break;
                    case OpCode.Return:
                        OpReturn();
                        break;
                    case OpCode.Interop:
                        OpInterop();
                        break;
                    case OpCode.Jump:
                        OpJump();
                        break;
                    case OpCode.JumpIfTrue:
                        OpJumpIfTrue();
                        break;
                    case OpCode.Add:
                        OpAdd();
                        break;
                    case OpCode.Subtract:
                        break;
                    case OpCode.Divide:
                        break;
                    case OpCode.Multiply:
                        break;
                    case OpCode.Modulo:
                        break;
                    case OpCode.Equal:
                        break;
                    case OpCode.Not:
                        break;
                    case OpCode.Method:
                    case OpCode.Exit:
                        OpExit();
                        break;
                    case OpCode.DebugPrint:
                        OpDebugPrint();
                        break;
                }

                if (Coroutine != null)
                {
                    yield return Coroutine;
                    Coroutine = null;
                }
            }
        }
    }
}
