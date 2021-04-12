namespace Luxembourg
{
    
        public class AssignExpression : Expression
        {
            public AssignExpression(Token name, Expression value)
            {
                Name = name;
                Value = value;
            }

            public Token Name { get; }
            public Expression Value { get; }

            public override T Accept<T>(IExpressionVisitor<T> visitor)
            {
                return visitor.VisitAssignExpression(this);
            }
        }
    
}