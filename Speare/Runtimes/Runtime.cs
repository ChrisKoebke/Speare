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

        public Stack<RuntimeScope> Stack = new Stack<RuntimeScope>();

        public byte[] Ops;
        public byte[] Chrh;
        public char[] Chrb;
        public byte[] Mth;

        public int Address = 0;
        public IEnumerator Coroutine;

        public RuntimeScope Scope
        {
            get { return Stack.Peek(); }
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

                fixed (char* buffer = Chrb)
                {
                    return new string(buffer, startIndex, length);
                }
            }
        }

        public object ReadRegister(byte index)
        {
            fixed (byte* pointer = Scope.Registers)
            {
                switch (*(DataType*)pointer)
                {
                    case DataType.Int:
                        return *(int*)(pointer + index * 5 + 1);
                    case DataType.Float:
                        return *(float*)(pointer + index * 5 + 1);
                    case DataType.ChrPointer:
                        return ReadString(*(int*)(pointer + index * 5 + 1));
                    default:
                        return null;
                }
            }
        }

        public void OpPushScope()
        {
            Stack.Push(new RuntimeScope(this));
        }

        public void OpPopScope()
        {
            Stack.Pop();
        }

        public void OpConstant()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* registersPointer = Scope.Registers)
            {
                *(DataType*)(registersPointer) = *(DataType*)(pointer + Address);
                *(int*)(registersPointer + 1) = *(int*)(pointer + Address + 1);
                Address += 5;
            }
        }

        public void OpStore()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* registersPointer = Scope.Registers)
            {
                var register = *(pointer + Address);
                Address++;

                *(DataType*)(registersPointer + register * 5) = *(DataType*)(registersPointer);
                *(int*)(registersPointer + register * 5 + 1) = *(int*)(registersPointer + 1);
            }
        }

        public void OpLoad()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* registersPointer = Scope.Registers)
            {
                var register = *(pointer + Address);
                Address++;

                *(DataType*)(registersPointer) = *(DataType*)(registersPointer + register * 5);
                *(int*)(registersPointer + 1) = *(int*)(registersPointer + register * 5 + 1);
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
                    parameters[i - 1] = ReadRegister(i);
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

        public void OpAdd()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* registersPointer = Scope.Registers)
            {
                var registerA = *(pointer + Address);
                var registerB = *(pointer + Address + 1);

                Address += 2;

                var typeA = *(DataType*)(registersPointer + registerA * 5);
                var typeB = *(DataType*)(registersPointer + registerB * 5);

                if (typeA == DataType.Int && typeB == DataType.Int)
                {
                    *(DataType*)(registersPointer) = DataType.Int;
                    *(int*)(registersPointer + 1) = *(int*)(registersPointer + registerA * 5 + 1) + *(int*)(registersPointer + registerB * 5 + 1);
                }
                else if (typeA == DataType.Int && typeB == DataType.Float)
                {
                    *(DataType*)(registersPointer) = DataType.Float;
                    *(float*)(registersPointer + 1) = *(int*)(registersPointer + registerA * 5 + 1) + *(float*)(registersPointer + registerB * 5 + 1);
                }
                else if (typeA == DataType.Float && typeB == DataType.Int)
                {
                    *(DataType*)(registersPointer) = DataType.Float;
                    *(float*)(registersPointer + 1) = *(float*)(registersPointer + registerA * 5 + 1) + *(int*)(registersPointer + registerB * 5 + 1);
                }
                else if (typeA == DataType.Float && typeB == DataType.Float)
                {
                    *(DataType*)(registersPointer) = DataType.Float;
                    *(float*)(registersPointer + 1) = *(float*)(registersPointer + registerA * 5 + 1) + *(float*)(registersPointer + registerB * 5 + 1);
                }
            }
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
                        OpStore();
                        break;
                    case OpCode.Load:
                        OpLoad();
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
