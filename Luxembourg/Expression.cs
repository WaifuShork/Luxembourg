using System.Collections.Generic;

namespace Luxembourg
{
    public abstract class Expression
    {
        public abstract T Accept<T>(Visitor<T> visitor);
        
        public interface Visitor<T>
        {
            T VisitBinaryExpression(Binary expression);
            T VisitGroupingExpression(Grouping expression);
            T VisitLiteralExpression(Literal expression);
            T VisitUnaryExpression(Unary expression);
            T VisitCallExpression(Call expression);
            T VisitGetExpression(Get expression);
            T VisitLogicalExpression(Logical expression);
            T VisitSetExpression(Set expression);
            T VisitBaseExpression(Base expression);
            T VisitThisExpression(This expression);
            T VisitVariableExpression(Variable expression);
            T VisitAssignExpression(Assign expression);
        }

        public class Assign : Expression
        {
            public Assign(Token name, Expression value)
            {
                Name = name;
                Value = value;
            }
            
            public Token Name { get; }
            public Expression Value { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitAssignExpression(this);
            }
        }

        public class Call : Expression
        {
            public Call(Expression callee, Token paren, List<Expression> arguments)
            {
                Callee = callee;
                Paren = paren;
                Arguments = arguments;
            }
            
            public Expression Callee { get; }
            public Token Paren { get; }
            public List<Expression> Arguments { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitCallExpression(this);
            }
        }
        
        public class Binary : Expression
        {
            public Binary(Expression left, Token op, Expression right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public Expression Left { get; }
            public Token Operator { get; }
            public Expression Right { get; }
        
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitBinaryExpression(this);
            }
        }

        public class Get : Expression
        {
            public Get(Expression obj, Token name)
            {
                Object = obj;
                Name = name;
            }

            public Expression Object { get; }
            public Token Name { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitGetExpression(this);
            }
        }

        public class Set : Expression
        {
            public Set(Expression obj, Token name, Expression value)
            {
                Object = obj;
                Name = name;
                Value = value;
            }
            
            public Expression Object { get; }
            public Token Name { get; }
            public Expression Value { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitSetExpression(this);
            }
        }

        public class Base : Expression
        {
            public Base(Token keyword, Token method)
            {
                Keyword = keyword;
                Method = method;
            }
            
            public Token Keyword { get; }
            public Token Method { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitBaseExpression(this);
            }
        }

        public class This : Expression
        {
            public This(Token keyword)
            {
                Keyword = keyword;
            }
            
            public Token Keyword { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitThisExpression(this);
            }
        }

        public class Variable : Expression
        {
            public Variable(Token name)
            {
                Name = name;
            }
            
            public Token Name { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitVariableExpression(this);
            }
        }

        public class Logical : Expression
        {
            public Logical(Expression left, Token op, Expression right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }
            
            public Expression Left { get; }
            public Token Operator { get; }
            public Expression Right { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitLogicalExpression(this);
            }
        }
        
        public class Grouping : Expression
        {
            public Grouping(Expression expression)
            {
                Expression = expression;
            }

            public Expression Expression { get; }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitGroupingExpression(this);
            }
        }
        
        public class Literal : Expression
        {
            public Literal(object value)
            {
                Value = value;
            }
        
            public object Value { get; }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitLiteralExpression(this);
            }
        }
        
        public class Unary : Expression
        {
            public Unary(Token op, Expression right)
            {
                Operator = op;
                Right = right;
            }
        
            public Token Operator { get; }
            public Expression Right { get; }

            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitUnaryExpression(this);
            }
        }
    }
}