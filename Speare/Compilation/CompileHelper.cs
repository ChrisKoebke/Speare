using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compilation
{
    public static class CompileHelper
    {
        public static void BuildChr(string[] values, out byte[] chrHeader, out string chrBuffer)
        {
            var builder = new StringBuilder();

            using (var memory = new MemoryStream())
            using (var writer = new BinaryWriter(memory))
            {
                int index = 0;
                for (int i = 0; i < values.Length; i++)
                {
                    writer.Write(index);
                    writer.Write(values[i].Length);
                    builder.Append(values[i]);

                    index += values[i].Length;
                }

                chrHeader = memory.ToArray();
                chrBuffer = builder.ToString();
            }
        }
    }
}
