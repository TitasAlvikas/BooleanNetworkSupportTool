using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanNetworkSupportTool
{
    public class OperandToken : Token
    {
    }
    public class OrToken : OperandToken
    {
    }

    public class AndToken : OperandToken
    {
    }

    public class ParenthesisToken : Token
    {
    }

    public class ClosedParenthesisToken : ParenthesisToken
    {
    }

    public class OpenParenthesisToken : ParenthesisToken
    {
    }

    public class NegationToken : Token
    {
    }

    public class VariableToken : Token
    {
        string name;

        public VariableToken(string newName)
        {
            name = newName;
        }        

        public string getName()
        {
            return name;
        }
    }

    public abstract class Token
    {
    }
}
