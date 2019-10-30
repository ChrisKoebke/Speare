using Speare.Compilation;
using Speare.Parser;
using Speare.Runtime;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Testing
{
    class Program
    {
        public static void PrintVector(string x, int y, int z)
        {
            Console.WriteLine("Vector " + x + "x" + y + "x" + z);
        }

        static OpBuilder Test1()
        {
            return new OpBuilder()

            .Method()
                .Constant(Register.R0, 0)
                .Constant(Register.R1, 5)
                .Label(":loop")
                .Constant(Register.R2, 1)
                .Add(Register.R0, Register.R2)
                .Set(Register.R0, Register.LastResult)
                .DebugPrint(Register.R0)
                .Compare(Register.R0, Register.R1, Comparison.S)
                .JumpIf(":loop")
                .Constant(Register.R3, "We made it through the loop!")
                .DebugPrint(Register.R3)
                .Constant(Register.Param0, "Another test")
                .Call(methodIndex: 1)
                .DebugPrint(Register.LastResult)
                .GlobalRead(Register.Param0, "TestVar")
                .Interop("PrintVector")
            .Method(parameterCount: 1)
                .GlobalRead(Register.Local0, "TestVar")
                .DebugPrint(Register.Param0)
                .Constant(Register.LastResult, 199)
                .Return();
        }

        static OpBuilder TestGlobalReadWrite()
        {
            return new OpBuilder()

            .Method()
                .GlobalRead(Register.Local0, "TestVar")
                .Constant(Register.Local1, 100)
                .Add(Register.Local0, Register.Local1)
                .GlobalWrite("TestVar", Register.LastResult);
        }

        static void Main(string[] args)
        {
            Interop.RegisterMethodsOf<Program>();

            var vm = VM.FromBuilder(TestGlobalReadWrite());

            vm.Allocate();
            vm["TestVar"] = 33;

            vm.Run(methodIndex: 0).MoveNext();

            Console.WriteLine(vm["TestVar"]);

            Console.ReadLine();
        }
    }
}
