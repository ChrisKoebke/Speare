using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Utility
{
    public static class StringExtensions
    {
        public static unsafe int GetReliableHashCode(this string value)
        {
            fixed (char* src = value)
            {
                Contract.Assert(src[value.Length] == '\0', "src[this.Length] == '\\0'");
                Contract.Assert(((int)src) % 4 == 0, "Managed string should start at 4 bytes boundary");

                int hash1 = (5381<<16) + 5381;
                int hash2 = hash1;

                int* pint = (int *)src;
                int len = value.Length;
                while (len > 2)
                {
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                    hash2 = ((hash2 << 5) + hash2 + (hash2 >> 27)) ^ pint[1];
                    pint += 2;
                    len  -= 4;
                }
 
                if (len > 0)
                {
                    hash1 = ((hash1 << 5) + hash1 + (hash1 >> 27)) ^ pint[0];
                }

                return hash1 + (hash2 * 1566083941);
            }
        }
    }
}
