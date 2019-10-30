using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Speare.Parser
{
    public enum TokenType
    {
        None,
        BeginBlock,
        EndBlock,
        Method,
        Identifier,
        ParameterSeparator,
        OpenParenthesis,
        CloseParenthesis,
        Operator,
        Integer,
        Float,
        String,
        Speaker,
        BeginGameEvent,
        EndGameEvent,
        GameEvent,
        Error,
        EOF
    }
}
