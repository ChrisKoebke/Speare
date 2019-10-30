using Speare.Utility;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Runtime
{
    public static class Interop
    {
        internal static Dictionary<int, MethodInfo> Methods = new Dictionary<int, MethodInfo>();
        internal static Dictionary<int, object[]> ParameterPool = new Dictionary<int, object[]>();

        public static void RegisterMethodsOf<T>()
        {
            foreach (var method in typeof(T).GetMethods())
            {
                Methods[method.Name.GetHashCode32()] = method;
                ParameterPool[method.Name.GetHashCode32()] = new object[method.GetParameters().Length];
            }
        }
    }
}
