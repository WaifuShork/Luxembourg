namespace Luxembourg
{
    
        public class SetExpression : Expression
        {
            public SetExpression(Expression obj, Token name, Expression value)
            {
                Object = obj;
                Name = name;
                Value = value;
            }

            public Expression Object { get; }
            public Token Name { get; }
            public Expression Value { get; }

            public override T Accept<T>(IExpressionVisitor<T> visitor)
            {
                return visitor.VisitSetExpression(this);
            }
        }
    
}