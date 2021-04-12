namespace Luxembourg
{
    
        public class GetExpression : Expression
        {
            public GetExpression(Expression obj, Token name)
            {
                Object = obj;
                Name = name;
            }

            public Expression Object { get; }
            public Token Name { get; }

            public override T Accept<T>(IExpressionVisitor<T> visitor)
            {
                return visitor.VisitGetExpression(this);
            }
        }
    
}