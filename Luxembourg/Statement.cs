using System.Collections.Generic;

namespace Luxembourg
{
    public abstract class Statement
    {
        public abstract T Accept<T>(Visitor<T> visitor);
        
        public interface Visitor<T>
        {
            T VisitBlockStatement(Block statement);
            T VisitClassStatement(Class statement);
            T VisitExpressionStatement(Expression statement);
            T VisitFunctionStatement(Function statement);
            T VisitIfStatement(If statement);
            T VisitReturnStatement(Return statement);
            T VisitPrintStatement(Print statement);
            T VisitVarStatement(Var statement);
            T VisitWhileStatement(While statement);
        }

        public class Block : Statement
        {
            public Block(List<Statement> statements)
            {
                Statements = statements;
            }
            
            public List<Statement> Statements { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitBlockStatement(this);
            }
        }

        public class Class : Statement
        {
            
            public Class(Token name, Luxembourg.Expression.Variable baseClass, List<Function> methods)
            {
                Name = name;
                BaseClass = baseClass;
                Methods = methods;
            }
            
            public Token Name { get; }
            public Luxembourg.Expression.Variable BaseClass { get; }
            public List<Function> Methods { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitClassStatement(this);
            }
        }

        public class Expression : Statement
        {
            public Expression(Luxembourg.Expression expression)
            {
                ExpressionSt = expression;
            }
            
            public Luxembourg.Expression ExpressionSt { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitExpressionStatement(this);
            }
        }

        public class Function : Statement
        {
            public Function(Token name, List<Token> parameters, List<Statement> body)
            {
                Name = name;
                Parameters = parameters;
                Body = body;
            }
            
            public Token Name { get; }
            public List<Token> Parameters { get; }
            public List<Statement> Body { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitFunctionStatement(this);
            }
        }

        public class If : Statement
        {
            public If(Luxembourg.Expression condition, Statement thenBranch, Statement elseBranch)
            {
                Condition = condition;
                ThenBranch = thenBranch;
                ElseBranch = elseBranch;
            }
            
            public Luxembourg.Expression Condition { get; }
            public Statement ThenBranch { get; }
            public Statement ElseBranch { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitIfStatement(this);
            }
        }

        public class Print : Statement
        {
            public Print(Luxembourg.Expression expression)
            {
                Expression = expression;
            }
            
            public Luxembourg.Expression Expression { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitPrintStatement(this);
            }
        }

        public class Return : Statement
        {
            public Return(Token keyword, Luxembourg.Expression value)
            {
                Keyword = keyword;
                Value = value;
            }
            
            public Token Keyword { get; }
            public Luxembourg.Expression Value { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitReturnStatement(this);
            }
        }

        public class Var : Statement
        {
            public Var(Token name, Luxembourg.Expression initializer)
            {
                Name = name;
                Initializer = initializer;
            }
            
            public Token Name { get; }
            public Luxembourg.Expression Initializer { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitVarStatement(this);
            }
        }

        public class While : Statement
        {
            public While(Luxembourg.Expression condition, Statement body)
            {
                Condition = condition;
                Body = body;
            }
            
            public Luxembourg.Expression Condition { get; }
            public Statement Body { get; }
            
            public override T Accept<T>(Visitor<T> visitor)
            {
                return visitor.VisitWhileStatement(this);
            }
        }
    }
}