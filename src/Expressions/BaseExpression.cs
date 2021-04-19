namespace Luxembourg.Expressions
{
    public class BaseExpression : Expression
    {
        public BaseExpression(Token keyword, Token method)
        {
            Keyword = keyword;
            Method = method;
        }

        public Token Keyword { get; }
        public Token Method { get; }

        public override T Accept<T>(IExpressionVisitor<T> visitor)
        {
            return visitor.VisitBaseExpression(this);
        }
    }
}