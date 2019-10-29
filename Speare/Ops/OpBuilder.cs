using Speare.Runtimes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Ops
{
    public unsafe class OpBuilder
    {
        public OpBuilder()
        {
            _ops = new BinaryWriter(_opsStream);
            _chrh = new BinaryWriter(_chrhStream);
            _chrb = new BinaryWriter(_chrbStream);
        }

        private MemoryStream _opsStream = new MemoryStream();
        private BinaryWriter _ops;

        private int _chrhAddress = 0;
        private int _chrbOpAddress = 0;

        private MemoryStream _chrhStream = new MemoryStream();
        private BinaryWriter _chrh;

        private MemoryStream _chrbStream = new MemoryStream();
        private BinaryWriter _chrb;

        public OpBuilder PushScope()
        {
            _ops.Write((short)OpCode.PushScope);
            return this;
        }

        public OpBuilder PopScope()
        {
            _ops.Write((short)OpCode.PopScope);
            return this;
        }

        public OpBuilder Constant(Var var, int value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)var);
            _ops.Write((byte)DataType.Int);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(Var var, float value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)var);
            _ops.Write((byte)DataType.Float);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(Var var, string value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)var);
            _ops.Write((byte)DataType.ChrPointer);
            _ops.Write(_chrhAddress);

            _chrh.Write(_chrbOpAddress);
            _chrh.Write(value.Length);

            fixed (char* pointer = value)
            {
                for (int i = 0; i < value.Length; i++)
                {
                    _chrb.Write(*(pointer + i));
                }
            }

            _chrbOpAddress += value.Length;
            _chrhAddress += 1;

            return this;
        }

        public OpBuilder Move(Var var)
        {
            return Move((byte)var);
        }

        public OpBuilder Move(byte var)
        {
            _ops.Write((short)OpCode.Move);
            _ops.Write(var);
            return this;
        }

        public OpBuilder Load(Var var)
        {
            return Load((byte)var);
        }

        public OpBuilder Load(byte var)
        {
            _ops.Write((short)OpCode.Load);
            _ops.Write(var);
            return this;
        }

        public OpBuilder Compare(Var a, Var b, Comparison comparison)
        {
            _ops.Write((short)OpCode.Compare);
            _ops.Write((byte)a);
            _ops.Write((byte)b);
            _ops.Write((byte)comparison);
            return this;
        }

        public OpBuilder Interop(string methodName)
        {
            _ops.Write((short)OpCode.Interop);
            _ops.Write(methodName.GetHashCode());

            return this;
        }

        public OpBuilder Jump(int address)
        {
            _ops.Write((short)OpCode.Jump);
            _ops.Write(address);
            return this;
        }

        public OpBuilder JumpIf(int address)
        {
            _ops.Write((short)OpCode.JumpIf);
            _ops.Write(address);
            return this;
        }

        public OpBuilder Add(Var varA, Var varB)
        {
            return Add((byte)varA, (byte)varB);
        }

        public OpBuilder Add(byte varA, byte varB)
        {
            _ops.Write((short)OpCode.Add);
            _ops.Write(varA);
            _ops.Write(varB);
            return this;
        }

        public OpBuilder DebugPrint(Var var)
        {
            return DebugPrint((byte)var);
        }

        public OpBuilder DebugPrint(byte var)
        {
            _ops.Write((short)OpCode.DebugPrint);
            _ops.Write(var);
            return this;
        }

        public void Build(out byte[] ops, out byte[] chrh, out byte[] chrb, out byte[] mth)
        {
            ops = _opsStream.ToArray();
            chrh = _chrhStream.ToArray();
            chrb = _chrbStream.ToArray();
            mth = new byte[0];
        }
    }
}
