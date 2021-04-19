using System.Collections.Generic;

namespace Luxembourg.Expressions
{
    public class CallExpression : Expression
    {
        public CallExpression(Expression callee, Token paren, List<Expression> arguments)
        {
            Callee = callee;
            Paren = paren;
            Arguments = arguments;
        }

        public Expression Callee { get; }
        public Token Paren { get; }
        public List<Expression> Arguments { get; }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitCallExpression(this);
        }
    }
}