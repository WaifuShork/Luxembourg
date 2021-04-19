using Luxembourg.Expressions;

namespace Luxembourg.Statements
{
    public class VarStatement : Statement
    {
        public VarStatement(Token name, Expression initializer)
        {
            Name = name;
            Initializer = initializer;
        }

        public Token Name { get; }
        public Expression Initializer { get; }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitVarStatement(this);
        }
    }
}