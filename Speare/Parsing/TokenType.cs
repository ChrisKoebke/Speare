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
        BeginGameEvent,
        EndGameEvent,
        GameEvent,
        Integer,
        Float,
        String,
        Error,
        EOF
    }
}
