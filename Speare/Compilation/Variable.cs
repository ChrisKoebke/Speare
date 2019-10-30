using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compilation
{
    [StructLayout(LayoutKind.Explicit)]
    public ref struct Variable
    {
        [FieldOffset(0)]
        public DataType Type;

        [FieldOffset(1)]
        public int IntValue;

        [FieldOffset(1)]
        public float FloatValue;

        [FieldOffset(1)]
        public bool BoolValue;
    }
}
