using Luxembourg.Expressions;

namespace Luxembourg.Statements
{
    public class ReturnStatement : Statement
    {
        public ReturnStatement(Token keyword, Expression value)
        {
            Keyword = keyword;
            Value = value;
        }

        public Token Keyword { get; }
        public Expression Value { get; }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitReturnStatement(this);
        }
    }
}