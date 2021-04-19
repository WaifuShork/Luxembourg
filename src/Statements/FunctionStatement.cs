using System.Collections.Generic;

namespace Luxembourg.Statements
{
    public class FunctionStatement : Statement
    {
        public FunctionStatement(Token name, List<Token> parameters, List<Statement> body)
        {
            Name = name;
            Parameters = parameters;
            Body = body;
        }

        public Token Name { get; }
        public List<Token> Parameters { get; }
        public List<Statement> Body { get; }

        public override T Accept<T>(IStatementVisitor<T> visitor)
        {
            return visitor.VisitFunctionStatement(this);
        }
    }
}