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

        public string ReadString(int index)
        {
            fixed (byte* headerPointer = Chrh)
            {
                int startIndex = *(int*)(headerPointer + index * 8);
                int length = *(int*)(headerPointer + index * 8 + 4);

                fixed (byte* buffer = Chrb)
                {
                    return new string((sbyte*)buffer, startIndex, length, Encoding.Default);
                }
            }
        }

        public object ReadFromScope(byte index)
        {
            fixed (byte* scope = Scope)
            {
                switch (*(DataType*)(scope + index * 5))
                {
                    case DataType.Int:
                        return *(int*)(scope + index * 5 + 1);
                    case DataType.Float:
                        return *(float*)(scope + index * 5 + 1);
                    case DataType.ChrPointer:
                        return ReadString(*(int*)(scope + index * 5 + 1));
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

        public void OpLoad()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                var register = *(pointer + Address);
                Address++;

                *(DataType*)(scope) = *(DataType*)(scope + register * 5);
                *(int*)(scope + 1) = *(int*)(scope + register * 5 + 1);
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

                *(DataType*)(scope) = DataType.Int;
                *(int*)(scope + 1) = (int)ReadFromScope(a) < (int)ReadFromScope(b) ? 1 : 0;

                Address += 1;
            }
        }

        public void OpCall()
        {
            fixed (byte* pointer = Ops)
            {
                var methodAddress = *(int*)(pointer + Address);
                Address += 4;

                // TODO: Resolve
            }
        }

        public void OpInterop()
        {
            fixed (byte* pointer = Ops)
            {
                var hash = *(int*)(pointer + Address);

                var info = Interop.Methods[hash];
                var parameters = Interop.ParameterPool[hash];

                for (byte i = 1; i <= parameters.Length; i++)
                {
                    parameters[i - 1] = ReadFromScope(i);
                }

                info.Invoke(null, parameters);
            }
        }

        public void OpJump()
        {
            fixed (byte* pointer = Ops)
            {
                Address = *(int*)(pointer + Address);
            }
        }

        public void OpJumpIf()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                // Last result is false
                if (*(int*)(scope + 1) == 0)
                {
                    Address += 4;
                    return;
                }

                Address = *(int*)(pointer + Address);
            }
        }

        public void OpAdd()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* scope = Scope)
            {
                var registerA = *(pointer + Address);
                var registerB = *(pointer + Address + 1);

                Address += 2;

                var typeA = *(DataType*)(scope + registerA * 5);
                var typeB = *(DataType*)(scope + registerB * 5);

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

        public void OpDebugPrint()
        {
            fixed (byte* pointer = Ops)
            {
                var register = *(pointer + Address);
                Address++;

                Console.WriteLine(ReadFromScope(register));
            }
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
                    case OpCode.Interop:
                        OpInterop();
                        break;
                    case OpCode.Jump:
                        OpJump();
                        break;
                    case OpCode.JumpIf:
                        OpJumpIf();
                        break;
                    case OpCode.Add:
                        OpAdd();
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
