namespace Luxembourg.Expressions
{
    public class UnaryExpression : Expression
    {
        public UnaryExpression(Token op, Expression right)
        {
            Operator = op;
            Right = right;
        }

        public Token Operator { get; }
        public Expression Right { get; }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitUnaryExpression(this);
        }
    }
}