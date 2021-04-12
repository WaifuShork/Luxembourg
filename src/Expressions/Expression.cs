using System.Collections.Generic;

namespace Luxembourg
{
    public abstract partial class Expression
    {
        public abstract T Accept<T>(IExpressionVisitor<T> visitor);
    }
}