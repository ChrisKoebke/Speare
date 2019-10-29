using Speare.Ops;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtimes
{
    public class RuntimeScope : IDisposable
    {
        public RuntimeScope(Runtime runtime)
        {
            Runtime = runtime;
        }

        public Runtime Runtime;
        public object Result;

        public DataType ValueType;
        public int Value;

        public byte[] Registers = new byte[4 * 16];
        public byte[] Vars = new byte[4 * 32];

        public void Dispose()
        {
            Runtime.Stack.Pop();
        }
    }
}
