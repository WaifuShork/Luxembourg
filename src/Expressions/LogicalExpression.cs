﻿namespace Luxembourg
{
    
        public class LogicalExpression : Expression
        {
            public LogicalExpression(Expression left, Token op, Expression right)
            {
                Left = left;
                Operator = op;
                Right = right;
            }

            public Expression Left { get; }
            public Token Operator { get; }
            public Expression Right { get; }

            public override T Accept<T>(IExpressionVisitor<T> visitor)
            {
                return visitor.VisitLogicalExpression(this);
            }
        }
    
}