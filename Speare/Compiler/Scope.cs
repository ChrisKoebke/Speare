using Speare.Runtime;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Compiler
{
    public class Scope
    {
        private Dictionary<string, Register> _nameToRegister = new Dictionary<string, Register>();
        private Dictionary<Register, string> _registerToName = new Dictionary<Register, string>();

        public Register Get(string varName)
        {
            Register result;
            _nameToRegister.TryGetValue(varName, out result);
            return result;
        }

        public string Get(Register register)
        {
            string result;
            _registerToName.TryGetValue(register, out result);
            return result;
        }

        public void Set(string varName, Register register)
        {
            _nameToRegister[varName] = register;
            _registerToName[register] = varName;
        }

        public void Free(string varName)
        {
            if (!_nameToRegister.ContainsKey(varName))
                return;

            _registerToName.Remove(_nameToRegister[varName]);
            _nameToRegister.Remove(varName);
        }

        public void Free(Register register)
        {
            if (!_registerToName.ContainsKey(register))
                return;

            _nameToRegister.Remove(_registerToName[register]);
            _registerToName.Remove(register);
        }
    }
}
