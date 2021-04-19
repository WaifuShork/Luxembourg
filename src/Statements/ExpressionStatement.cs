using Luxembourg.Expressions;

namespace Luxembourg.Statements
{
    public class ExpressionStatement : Statement
    {
        public ExpressionStatement(Expression expression)
        {
            Expression = expression;
        }

        public Expression Expression { get; }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitExpressionStatement(this);
        }
    }
}