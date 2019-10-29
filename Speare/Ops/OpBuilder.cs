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
            _opsWriter = new BinaryWriter(_opsStream);
            _chrhWriter = new BinaryWriter(_chrhStream);
        }

        private MemoryStream _opsStream = new MemoryStream();
        private BinaryWriter _opsWriter;

        private int _chrhAddress = 0;
        private int _chrbIndex = 0;
        private MemoryStream _chrhStream = new MemoryStream();
        private BinaryWriter _chrhWriter;

        private StringBuilder _chrb = new StringBuilder();
        
        public OpBuilder Constant(int value)
        {
            _opsWriter.Write((int)OpCode.Constant);
            _opsWriter.Write((byte)DataType.Int);
            _opsWriter.Write(value);
            return this;
        }

        public OpBuilder Constant(float value)
        {
            _opsWriter.Write((int)OpCode.Constant);
            _opsWriter.Write((byte)DataType.Float);
            _opsWriter.Write(value);
            return this;
        }

        public OpBuilder Constant(string value)
        {
            _opsWriter.Write((int)OpCode.Constant);
            _opsWriter.Write((byte)DataType.ChrPointer);
            _opsWriter.Write(_chrhAddress);

            _chrhWriter.Write(_chrbIndex);
            _chrhWriter.Write(value.Length);
            _chrb.Append(value);

            _chrbIndex += value.Length;
            _chrhAddress += 1;

            return this;
        }

        public OpBuilder Store(byte registerIndex)
        {
            _opsWriter.Write((int)OpCode.Store);
            _opsWriter.Write(registerIndex);
            return this;
        }

        public OpBuilder Load(byte registerIndex)
        {
            _opsWriter.Write((int)OpCode.Load);
            _opsWriter.Write(registerIndex);
            return this;
        }

        public OpBuilder DebugPrint()
        {
            _opsWriter.Write((int)OpCode.DebugPrint);
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
