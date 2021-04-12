namespace Luxembourg
{
    
        public class ThisExpression : Expression
        {
            public ThisExpression(Token keyword)
            {
                Keyword = keyword;
            }

            public Token Keyword { get; }

            public override T Accept<T>(IExpressionVisitor<T> visitor)
            {
                return visitor.VisitThisExpression(this);
            }
        }
    
}