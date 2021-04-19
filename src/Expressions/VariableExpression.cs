namespace Luxembourg.Expressions
{
    public class VariableExpression : Expression
    {
        public VariableExpression(Token name)
        {
            Name = name;
        }

        public Token Name { get; }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitVariableExpression(this);
        }
    }
}