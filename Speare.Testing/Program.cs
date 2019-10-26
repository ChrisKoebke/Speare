using Speare.Parsing;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Testing
{
    class Program
    {
        static void Main(string[] args)
        {
            var code = "Tobi: \"Hey doc!\"\nDoc: \"Hey Tobi\"";

            var span = code.ToSpan().Split('\n');
            var index = 0;
            
            foreach (var line in span)
            {
                index++;
            }

            Console.ReadLine();
        }
    }
}
