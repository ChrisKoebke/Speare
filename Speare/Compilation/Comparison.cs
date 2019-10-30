using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compilation
{
    public enum Comparison : byte
    {
        None,
        Equal,
        NotEqual,
        LargerThan,
        SmallerThan,
        LargerOrEqualThan,
        SmallerOrEqualThan
    }
}
