using Speare.Compiler;
using Speare.Runtime;
using Speare.Utility;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public unsafe class OpBuilder
    {
        private Memory _ops = new Memory();
        private Memory _chrh = new Memory();
        private Memory _chrb = new Memory();
        private Memory _mth = new Memory();
        
        private int _chrhAddress = 0;
        private int _chrbOpAddress = 0;

        private Dictionary<string, int> _labels = new Dictionary<string, int>();

        public void Clear()
        {
            _ops.Clear();
            _chrh.Clear();
            _chrb.Clear();
            _mth.Clear();

            _chrhAddress = 0;
            _chrbOpAddress = 0;

            _labels.Clear();
        }

        public OpBuilder PushScope()
        {
            _ops.Write((short)Op.PushScope);
            return this;
        }

        public OpBuilder PopScope()
        {
            _ops.Write((short)Op.PopScope);
            return this;
        }

        public OpBuilder Constant(Register reg, bool value)
        {
            _ops.Write((short)Op.Constant);
            _ops.Write((byte)reg);
            _ops.Write((byte)DataType.Bool);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(Register reg, int value)
        {
            _ops.Write((short)Op.Constant);
            _ops.Write((byte)reg);
            _ops.Write((byte)DataType.Int);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(Register reg, float value)
        {
            _ops.Write((short)Op.Constant);
            _ops.Write((byte)reg);
            _ops.Write((byte)DataType.Float);
            _ops.Write(value);
            return this;
        }

        public OpBuilder Constant(Register reg, string value)
        {
            _ops.Write((short)Op.Constant);
            _ops.Write((byte)reg);
            _ops.Write((byte)DataType.String);
            _ops.Write(_chrhAddress);

            _chrh.Write(_chrbOpAddress);
            _chrh.Write(value.Length);

            _chrb.Write(value);
            
            _chrbOpAddress += value.Length;
            _chrhAddress += 1;

            return this;
        }

        public OpBuilder GlobalRead(Register reg, string name)
        {
            return GlobalRead(reg, name.GetHashCode32());
        }

        public OpBuilder GlobalRead(Register reg, int hash)
        {
            _ops.Write((short)Op.GlobalRead);
            _ops.Write((byte)reg);
            _ops.Write(hash);

            return this;
        }

        public OpBuilder GlobalWrite(string name, Register reg)
        {
            return GlobalWrite(name.GetHashCode32(), reg);
        }

        public OpBuilder GlobalWrite(int hash, Register reg)
        {
            _ops.Write((short)Op.GlobalWrite);
            _ops.Write(hash);
            _ops.Write((byte)reg);

            return this;
        }

        public OpBuilder Method(int parameterCount = 0)
        {
            _ops.Write((short)Op.MethodDefinition);

            _mth.Write((short)_ops.Position);
            _mth.Write((byte)parameterCount);

            return this;
        }

        public OpBuilder Label(string name)
        {
            _labels[name] = _ops.Position;
            return this;
        }
        
        public OpBuilder Set(Register destination, Register source)
        {
            _ops.Write((short)Op.Set);
            _ops.Write((byte)destination);
            _ops.Write((byte)source);
            return this;
        }

        public OpBuilder Interop(string methodName)
        {
            _ops.Write((short)Op.Interop);
            _ops.Write(methodName.GetHashCode32());

            return this;
        }

        public OpBuilder Jump(string label)
        {
            return Jump(_labels[label]);
        }

        public OpBuilder Jump(int address)
        {
            _ops.Write((short)Op.Jump);
            _ops.Write(address);
            return this;
        }

        public OpBuilder JumpIf(string label)
        {
            return JumpIf(_labels[label]);
        }

        public OpBuilder JumpIf(int address)
        {
            _ops.Write((short)Op.JumpIf);
            _ops.Write(address);
            return this;
        }

        public OpBuilder Arithmetic(Register regA, Register regB, Arithmetic op)
        {
            _ops.Write((short)Op.Arithmetic);
            _ops.Write((byte)regA);
            _ops.Write((byte)regB);
            _ops.Write((byte)op);
            return this;
        }

        public OpBuilder Call(int methodIndex)
        {
            _ops.Write((short)Op.Call);
            _ops.Write((short)methodIndex);

            return this;
        }

        public OpBuilder Return()
        {
            _ops.Write((short)Op.Return);
            return this;
        }

        public OpBuilder Exit()
        {
            _ops.Write((short)Op.Exit);
            return this;
        }

        public OpBuilder DebugPrint(Register reg)
        {
            return DebugPrint((byte)reg);
        }

        public OpBuilder DebugPrint(byte reg)
        {
            _ops.Write((short)Op.DebugPrint);
            _ops.Write(reg);
            return this;
        }

        public void Build(byte[] result)
        {
            fixed (byte* pointer = result)
            {
                // Op address
                *(int*)(pointer) = Constants.SizeOfHeader;
                // Mth address
                *(int*)(pointer + 4) = Constants.SizeOfHeader + _ops.Position;
                // Chrh address
                *(int*)(pointer + 8) = Constants.SizeOfHeader + _ops.Position + _mth.Position;
                // Chrb address
                *(int*)(pointer + 12) = Constants.SizeOfHeader + _ops.Position + _mth.Position + _chrh.Position;
            }

            Buffer.BlockCopy(_ops.Data, 0, result, Constants.SizeOfHeader, _ops.Position);
            Buffer.BlockCopy(_mth.Data, 0, result, Constants.SizeOfHeader + _ops.Position, _mth.Position);
            Buffer.BlockCopy(_chrh.Data, 0, result, Constants.SizeOfHeader + _ops.Position + _mth.Position, _chrh.Position);
            Buffer.BlockCopy(_chrb.Data, 0, result, Constants.SizeOfHeader + _ops.Position + _mth.Position + _chrh.Position, _chrb.Position);
        }

        public byte[] Build()
        {
            var result = new byte[16 + _ops.Position + _mth.Position + _chrh.Position + _chrb.Position];
            Build(result);

            return result;
        }
    }
}
