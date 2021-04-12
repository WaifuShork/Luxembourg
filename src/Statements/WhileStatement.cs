namespace Luxembourg
{
    
        public class WhileStatement : Statement
        {
            public WhileStatement(Expression condition, Statement body)
            {
                Condition = condition;
                Body = body;
            }

            public Expression Condition { get; }
            public Statement Body { get; }

            public override T Accept<T>(IStatementVisitor<T> visitor)
            {
                return visitor.VisitWhileStatement(this);
            }
        }
    
}