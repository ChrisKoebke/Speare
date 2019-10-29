using Speare.Ops;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtimes
{
    public unsafe class RuntimeScope : IDisposable
    {
        public RuntimeScope(Runtime runtime)
        {
            Runtime = runtime;
        }

        public Runtime Runtime;
        public object Result;
        public int ReturnAddress;

        public byte[] Registers = new byte[5 * 16];
        public byte[] Vars = new byte[5 * 32];

        public void Dispose()
        {
            Runtime.Stack.Pop();
        }
    }
}
