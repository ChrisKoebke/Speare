using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Parsing
{
    public enum TokenType
    {
        None,
        BeginBlock,
        EndBlock,
        Speaker,
        Sentence,
        MethodDefinition,
        MethodCall,
        Identifier,
        ParameterSeparator,
        BeginParameters,
        EndParameters,
        Operator,
        Integer,
        Float,
        String,
        BeginGameEvent,
        EndGameEvent,
        GameEvent,
        Error,
        EOF
    }
}
