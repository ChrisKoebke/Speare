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
        private ArrayBuilder _ops = new ArrayBuilder();
        private ArrayBuilder _chrh = new ArrayBuilder();
        private ArrayBuilder _chrb = new ArrayBuilder();
        
        private int _chrhAddress = 0;
        private int _chrbOpAddress = 0;

        private Dictionary<string, int> _labels = new Dictionary<string, int>();

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

        public OpBuilder Constant(Register reg, bool value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)reg);
            _ops.Write((byte)DataType.Bool);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(Register reg, int value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)reg);
            _ops.Write((byte)DataType.Int);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(Register reg, float value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)reg);
            _ops.Write((byte)DataType.Float);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(Register reg, string value)
        {
            _ops.Write((short)OpCode.Constant);
            _ops.Write((byte)reg);
            _ops.Write((byte)DataType.ChrPointer);
            _ops.Write(_chrhAddress);

            _chrh.Write(_chrbOpAddress);
            _chrh.Write(value.Length);

            _chrb.Write(value);
            
            _chrbOpAddress += value.Length;
            _chrhAddress += 1;

            return this;
        }

        public OpBuilder Label(string name)
        {
            _labels[name] = _ops.Position;
            return this;
        }
        
        public OpBuilder Move(Register source, Register destination)
        {
            _ops.Write((short)OpCode.Move);
            _ops.Write((byte)source);
            _ops.Write((byte)destination);
            return this;
        }
        
        public OpBuilder Compare(Register a, Register b, Comparison comparison)
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

        public OpBuilder Jump(string label)
        {
            return Jump(_labels[label]);
        }

        public OpBuilder Jump(int address)
        {
            _ops.Write((short)OpCode.Jump);
            _ops.Write(address);
            return this;
        }

        public OpBuilder JumpIf(string label)
        {
            return JumpIf(_labels[label]);
        }

        public OpBuilder JumpIf(int address)
        {
            _ops.Write((short)OpCode.JumpIf);
            _ops.Write(address);
            return this;
        }

        public OpBuilder Add(Register regA, Register regB)
        {
            _ops.Write((short)OpCode.Add);
            _ops.Write((byte)regA);
            _ops.Write((byte)regB);
            return this;
        }

        public OpBuilder DebugPrint(Register reg)
        {
            return DebugPrint((byte)reg);
        }

        public OpBuilder DebugPrint(byte reg)
        {
            _ops.Write((short)OpCode.DebugPrint);
            _ops.Write(reg);
            return this;
        }

        public void Build(out byte[] ops, out byte[] chrh, out byte[] chrb, out byte[] mth)
        {
            ops = _ops.Data;
            chrh = _chrh.Data;
            chrb = _chrb.Data;
            mth = new byte[0];
        }
    }
}
