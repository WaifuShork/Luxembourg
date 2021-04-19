using Luxembourg.Expressions;

namespace Luxembourg.Statements
{
    public class IfStatement : Statement
    {
        public IfStatement(Expression condition, Statement thenBranch, Statement elseBranch)
        {
            Condition = condition;
            ThenBranch = thenBranch;
            ElseBranch = elseBranch;
        }

        public Expression Condition { get; }
        public Statement ThenBranch { get; }
        public Statement ElseBranch { get; }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitIfStatement(this);
        }
    }
}