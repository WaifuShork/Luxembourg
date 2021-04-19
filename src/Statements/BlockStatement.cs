using System.Collections.Generic;

namespace Luxembourg.Statements
{
    public class BlockStatement : Statement
    {
        public BlockStatement(List<Statement> statements)
        {
            Statements = statements;
        }

        public List<Statement> Statements { get; }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitBlockStatement(this);
        } 
    }
}