using System.Collections.Generic;
using Luxembourg.Expressions;

namespace Luxembourg.Statements
{
    public class ClassStatement : Statement
    {
        public ClassStatement(Token name, VariableExpression baseClass, List<FunctionStatement> methods)
        {
            Name = name;
            BaseClass = baseClass;
            Methods = methods;
        }

        public Token Name { get; }
        public VariableExpression BaseClass { get; }
        public List<FunctionStatement> Methods { get; }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitClassStatement(this);
        }
    }
}