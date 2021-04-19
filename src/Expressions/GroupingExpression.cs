namespace Luxembourg.Expressions
{
    public class GroupingExpression : Expression
    {
        public GroupingExpression(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitGroupingExpression(this);
        }
    }
}