using Speare.Runtimes;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Ops
{
    public class OpBuilder
    {
        public OpBuilder()
        {
            _ops = new BinaryWriter(_opsStream);
            _chrh = new BinaryWriter(_chrhStream);
        }

        private MemoryStream _opsStream = new MemoryStream();
        private BinaryWriter _ops;

        private int _chrhAddress = 0;
        private int _chrbOpAddress = 0;
        private MemoryStream _chrhStream = new MemoryStream();
        private BinaryWriter _chrh;

        private StringBuilder _chrb = new StringBuilder();
        
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

        public OpBuilder Constant(int value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)DataType.Int);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(float value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)DataType.Float);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(string value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)DataType.ChrPointer);
            _ops.Write(_chrhAddress);

            _chrh.Write(_chrbOpAddress);
            _chrh.Write(value.Length);
            _chrb.Append(value);

            _chrbOpAddress += value.Length;
            _chrhAddress += 1;

            return this;
        }

        public OpBuilder Move(Register register)
        {
            return Move((byte)register);
        }

        public OpBuilder Move(byte register)
        {
            _ops.Write((short)OpCode.Move);
            _ops.Write(register);
            return this;
        }

        public OpBuilder Load(Register register)
        {
            return Load((byte)register);
        }

        public OpBuilder Load(byte register)
        {
            _ops.Write((short)OpCode.Load);
            _ops.Write(register);
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

        public OpBuilder Add(Register registerA, Register registerB)
        {
            return Add((byte)registerA, (byte)registerB);
        }

        public OpBuilder Add(byte registerA, byte registerB)
        {
            _ops.Write((short)OpCode.Add);
            _ops.Write(registerA);
            _ops.Write(registerB);
            return this;
        }

        public OpBuilder DebugPrint(Register register)
        {
            return DebugPrint((byte)register);
        }

        public OpBuilder DebugPrint(byte register)
        {
            _ops.Write((short)OpCode.DebugPrint);
            _ops.Write(register);
            return this;
        }

        public void Build(out byte[] ops, out byte[] chrh, out char[] chrb, out byte[] mth)
        {
            ops = _opsStream.ToArray();
            chrh = _chrhStream.ToArray();
            chrb = _chrb.ToString().ToCharArray();
            mth = null;
        }
    }
}
