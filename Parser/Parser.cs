using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BooleanNetworkSupportTool
{
    public class Parser
    {
        private readonly IEnumerator<Token> _tokens;

        public Parser(IEnumerable<Token> tokens)
        {
            _tokens = tokens.GetEnumerator();
            _tokens.MoveNext();
        }

        public Node Parse()
        {
            Node node = new Node();

            while (_tokens.Current != null)
            {
                if (_tokens.Current is OperandToken)
                {
                    throw new Exception("Invalid expression");
                }

                var isNegated = _tokens.Current is NegationToken;
                if (isNegated)
                {
                    node = new Node("!");
                    _tokens.MoveNext();
                }

                if (_tokens.Current is ParenthesisToken)
                {
                    if (isNegated)
                    {
                        node.AddChild(ParseParenthesis());
                    }
                    else
                    {
                        node = ParseParenthesis();
                    }
                }

                if(_tokens.Current is VariableToken)
                {
                    VariableToken varToken = (VariableToken) _tokens.Current;
                    
                    if (isNegated)
                    {
                        node.AddChild(new Node(varToken.getName()));
                    }
                    else
                    {
                        node = new Node(varToken.getName());
                    }

                    _tokens.MoveNext();
                }

                while (_tokens.Current is OperandToken)
                {
                    var operand = _tokens.Current;
                    if (!_tokens.MoveNext())
                    {
                        throw new Exception("Missing expression after operand");
                    }
                    Node secondBoolean = ParseParenthesis();

                    if (operand is AndToken)
                        node.SetParent(new Node("&"));
                    else
                        node.SetParent(new Node("|"));

                    node.parent.AddChild(secondBoolean);
                }

                return (node.parent is null) ? node : node.parent;
            }

            throw new Exception("Empty expression");
        }

        private Node ParseParenthesis()
        {
            if (_tokens.Current is OpenParenthesisToken)
            {
                _tokens.MoveNext();

                Node expInPars = Parse();

                if (!(_tokens.Current is ClosedParenthesisToken))
                    throw new Exception("Expecting Closing Parenthesis");

                _tokens.MoveNext();

                return expInPars;
            }
            if (_tokens.Current is ClosedParenthesisToken)
                throw new Exception("Unexpected Closed Parenthesis");

            // since its not an Expression in parenthesis, it must be an expression again
            Node expr = Parse();
            return expr;
        }
    }
}
