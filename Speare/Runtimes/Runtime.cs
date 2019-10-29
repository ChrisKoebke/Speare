using Speare.Ops;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
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

        public void Load(OpBuilder ops)
        {
            ops.Build(out Ops, out Chrh, out Chrb, out Mth);
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

        public object ReadConstant()
        {
            switch (Scope.ValueType)
            {
                case DataType.Int:
                    return Scope.Value;
                case DataType.Float:
                    fixed (int* pointer = &Scope.Value)
                    {
                        return *(float*)pointer;
                    }
                case DataType.ChrPointer:
                    return ReadString(Scope.Value);
                default:
                    return null;
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
            {
                Scope.ValueType = *(DataType*)(pointer + Address);
                Address++;
                Scope.Value = *(int*)(pointer + Address);
                Address += 4;
            }
        }

        public void OpStore()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* registersPointer = Scope.Registers)
            {
                var register = *(pointer + Address);
                Address++;

                *(DataType*)(registersPointer + register * 5) = Scope.ValueType;
                *(int*)(registersPointer + register * 5 + 1) = Scope.Value;
            }
        }

        public void OpLoad()
        {
            fixed (byte* pointer = Ops)
            fixed (byte* registersPointer = Scope.Registers)
            {
                var register = *(pointer + Address);
                Address++;

                Scope.ValueType = *(DataType*)(registersPointer + register * 5);
                Scope.Value = *(int*)(registersPointer + register * 5 + 1);
            }
        }

        public void OpCall()
        {
            fixed (byte* pointer = Ops)
            {
                Address = *(int*)(pointer + Address);
            }
        }

        public void OpJump()
        {
            fixed (byte* pointer = Ops)
            {
                Address = *(int*)(pointer + Address);
            }
        }

        public void OpDebugPrint()
        {
            Console.WriteLine(ReadConstant());
        }

        public unsafe OpCode Read()
        {
            fixed (byte* pointer = Ops)
            {
                var result = Unsafe.Read<OpCode>(pointer + Address);
                // Advance address by size of op code
                Address += 4;

                return result;
            }
        }

        public IEnumerator Run()
        { 
            while (Address < Ops.Length)
            {
                var op = Read();

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
                    case OpCode.Store:
                        OpStore();
                        break;
                    case OpCode.Load:
                        OpLoad();
                        break;
                    case OpCode.Call:
                        OpCall();
                        break;
                    case OpCode.Jump:
                        OpJump();
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

        public void GetMethodBoundaries(int methodAddress, out int startAddress, out int endAddress)
        {
            startAddress = 0;
            endAddress = 0;
        }
    }
}
