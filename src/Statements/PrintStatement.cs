namespace Luxembourg
{
    
        public class PrintStatement : Statement
        {
            public PrintStatement(Expression expression)
            {
                Expression = expression;
            }

            public Expression Expression { get; }

            public override T Accept<T>(IStatementVisitor<T> visitor)
            {
                return visitor.VisitPrintStatement(this);
            }
        }
    
}