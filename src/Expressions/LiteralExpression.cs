namespace Luxembourg
{
    
        public class LiteralExpression : Expression
        {
            public LiteralExpression(object value)
            {
                Value = value;
            }

            public object Value { get; }

            public override T Accept<T>(IExpressionVisitor<T> visitor)
            {
                return visitor.VisitLiteralExpression(this);
            }
        }
    
}