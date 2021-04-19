using System.Collections.Generic;

namespace Luxembourg.Statements
{
    public abstract partial class Statement
    {
        public abstract T Accept<T>(IStatementVisitor<T> visitor);
    }
}