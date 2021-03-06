using System.Collections.Generic;
using Luxembourg.Enums;
using Luxembourg.Errors;
using Luxembourg.Expressions;
using Luxembourg.Statements;

namespace Luxembourg
{
    public class Parser
    {
        private readonly List<Token> _tokens;
        private int _current = 0;

        public Parser(List<Token> tokens)
        {
            _tokens = tokens;
        }

        public List<Statement> Parse()
        {
            var statements = new List<Statement>();
            while (!IsAtEnd())
            {
                statements.Add(Declaration());
            }

            return statements;
        }

        private Statement Declaration()
        {
            try
            {
                if (Match(TokenType.Class))
                {
                    return ClassDeclaration();
                }
                
                if (Match(TokenType.Procedure))
                {
                    return Procedure("procedure");
                }

                if (Match(TokenType.Var))
                {
                    return VarDeclaration();
                }

                return Statement();
            }
            catch (ParseError)
            {
                Synchronize();
                return null;
            }
        }

        private Statement ClassDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect class name.");

            VariableExpression superclass = null;

            if (Match(TokenType.Less))
            {
                Consume(TokenType.Identifier, "Expect base class name.");
                superclass = new(Previous());
            }
            
            Consume(TokenType.OpenBrace, "Expect '{' before class body");

            var methods = new List<FunctionStatement>();

            while (!Check(TokenType.CloseBrace) && !IsAtEnd())
            {
                methods.Add(Procedure("method"));
            }

            Consume(TokenType.CloseBrace, "Expect '}' after class body.");
            return new ClassStatement(name, superclass, methods);
        }
        
        private FunctionStatement Procedure(string kind)
        {
            var name = Consume(TokenType.Identifier, $"Expect {kind} name.");
            Consume(TokenType.OpenParen, $"Expect '(' after {kind} name.");

            var parameters = new List<Token>();
            if (!Check(TokenType.CloseParen))
            {
                do
                {
                    if (parameters.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 parameters.");
                    }

                    parameters.Add(Consume(TokenType.Identifier, "Expect parameter name."));
                } 
                while (Match(TokenType.Comma));
            }

            Consume(TokenType.CloseParen, "Expect ')' after parameters.");
            
            Consume(TokenType.OpenBrace, "Expect '{' before " + kind + " body.");
            var body = Block();
            return new(name, parameters, body);
        }

        private Statement VarDeclaration()
        {
            var name = Consume(TokenType.Identifier, "Expect variable name.");

            Expression initializer = null;
            if (Match(TokenType.Equal))
            {
                initializer = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after variable declaration.");
            return new VarStatement(name, initializer);
        }
        
        private Expression Expression()
        {
            return Assignment();
        }

        private Expression Or()
        {
            var expression = And();

            while (Match(TokenType.Or))
            {
                var op = Previous();
                var right = And();
                expression = new LogicalExpression(expression, op, right);
            }

            return expression;
        }

        private Expression And()
        {
            var expression = Equality();

            while (Match(TokenType.And))
            {
                var op = Previous();
                var right = Equality();
                expression = new LogicalExpression(expression, op, right);
            }

            return expression;
        }

        private Expression Assignment()
        {
            var expression = Or();

            if (Match(TokenType.Equal))
            {
                var equals = Previous();
                var value = Assignment();

                if (expression is VariableExpression variable)
                {
                    var name = variable.Name;
                    return new AssignExpression(name, value);
                }
                else if (expression is GetExpression get)
                {
                    return new SetExpression(get.Object, get.Name, value);
                }

                Error(equals, "Invalid assignment target.");
            }

            return expression;
        }
        
        private Expression Equality()
        {
            var expression = Comparison();

            while (Match(TokenType.BangEquals, TokenType.EqualEqual))
            {
                var op = Previous();
                var right = Comparison();
                expression = new BinaryExpression(expression, op, right);
            }

            return expression;
        }
        
        private Expression Comparison()
        {
            var expression = Term();
            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var op = Previous();
                var right = Term();
                expression = new BinaryExpression(expression, op, right);
            }

            return expression;
        }

        private Expression Term()
        {
            var expression = Factor();

            while (Match(TokenType.Minus, TokenType.Plus))
            {
                var op = Previous();
                var right = Factor();
                expression = new BinaryExpression(expression, op, right);
            }

            return expression;
        }

        private Expression Factor()
        {
            var expression = Unary();

            while (Match(TokenType.Slash, TokenType.Star, TokenType.StarStar))
            {
                var op = Previous();
                var right = Unary();
                expression = new BinaryExpression(expression, op, right);
            }

            return expression;
        }

        private Expression Unary()
        {
            if (Match(TokenType.Bang, TokenType.Minus))
            {
                var op = Previous();
                var right = Unary();
                return new UnaryExpression(op, right);
            }

            return Call();
        }

        private Expression Call()
        {
            var expression = Primary();

            while (true)
            {
                if (Match(TokenType.OpenParen))
                {
                    expression = FinishCall(expression);
                }
                else if (Match(TokenType.Dot))
                {
                    var name = Consume(TokenType.Identifier, "Expect property name after '.'.");
                    expression = new GetExpression(expression, name);
                }
                else
                {
                    break;
                }
            }

            return expression;
        }

        private Expression FinishCall(Expression callee)
        {
            var arguments = new List<Expression>();

            if (!Check(TokenType.OpenParen))
            {
                do
                {
                    if (arguments.Count >= 255)
                    {
                        Error(Peek(), "Can't have more than 255 arguments");
                    }
                    arguments.Add(Expression());
                } 
                while (Match(TokenType.Comma));
            }

            var paren = Consume(TokenType.CloseParen, "Expect ')' after arguments");
            return new CallExpression(callee, paren, arguments);
        }

        private Expression Primary()
        {
            if (Match(TokenType.False))
            {
                return new LiteralExpression(false);
            }

            if (Match(TokenType.True))
            {
                return new LiteralExpression(true);
            }

            if (Match(TokenType.Nil))
            {
                return new LiteralExpression(null);
            }

            if (Match(TokenType.Number, TokenType.String))
            {
                return new LiteralExpression(Previous().Literal);
            }

            if (Match(TokenType.Base))
            {
                var keyword = Previous();
                Consume(TokenType.Dot, "Expect '.' after 'base'.");

                var method = Consume(TokenType.Identifier, "Expect base class method name.");
                return new BaseExpression(keyword, method);
            }

            if (Match(TokenType.This))
            {
                return new ThisExpression(Previous());
            }

            if (Match(TokenType.OpenParen))
            {
                // flagged
                var expression = Expression();
                Consume(TokenType.CloseParen, "Expect ')' after expression.");
                return new GroupingExpression(expression);
            }

            if (Match(TokenType.Identifier))
            {
                return new VariableExpression(Previous());
            }

            throw Error(Peek(), "Expected expression.");
        }
        
        private Statement Statement()
        {
            if (Match(TokenType.Print))
            {
                return PrintStatement();
            }

            if (Match(TokenType.OpenBrace))
            {
                return new BlockStatement(Block());
            }

            if (Match(TokenType.If))
            {
                return IfStatement();
            }

            if (Match(TokenType.While))
            {
                return WhileStatement();
            }

            if (Match(TokenType.For))
            {
                return ForStatement();
            }

            if (Match(TokenType.Return))
            {
                return ReturnStatement();
            }
            
            return ExpressionStatement();
        }

        private Statement ReturnStatement()
        {
            var keyword = Previous();
            Expression value = null;

            if (!Check(TokenType.Semicolon))
            {
                value = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after return value.");
            return new ReturnStatement(keyword, value);
        }

        private Statement ForStatement()
        {
            Consume(TokenType.OpenParen, "Expect '(' after 'for'.");

            Statement initializer;

            if (Match(TokenType.Semicolon))
            {
                initializer = null;
            }
            else if (Match(TokenType.Var))
            {
                initializer = VarDeclaration();
            }
            else
            {
                initializer = ExpressionStatement();
            }

            Expression condition = null;
            if (!Check(TokenType.Semicolon))
            {
                condition = Expression();
            }

            Consume(TokenType.Semicolon, "Expect ';' after loop condition.");

            Expression increment = null;
            if (!Check(TokenType.CloseParen))
            {
                increment = Expression();
            }

            Consume(TokenType.CloseParen, "Expect ')' after for clauses.");

            var body = Statement();

            if (increment != null)
            {
                body = new BlockStatement(new() { body, new ExpressionStatement(increment) });
            }

            if (condition == null)
            {
                condition = new LiteralExpression(true);
            }

            body = new WhileStatement(condition, body);

            if (initializer != null)
            {
                body = new BlockStatement(new() { initializer, body });
            }
            
            return body;
        }

        private Statement WhileStatement()
        {
            Consume(TokenType.OpenParen, "Expect '(' after 'while'.");
            var condition = Expression();
            Consume(TokenType.CloseParen, "Expect ')' after condition.");
            var body = Statement();

            return new WhileStatement(condition, body);
        }

        private Statement IfStatement()
        {
            Consume(TokenType.OpenParen, "Expect '(' after 'if'.");
            var condition = Expression();
            Consume(TokenType.CloseParen, "Expect ')' after expression.");

            var thenBranch = Statement();
            Statement elseBranch = null;
            
            if (Match(TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new IfStatement(condition, thenBranch, elseBranch);
        }

        private List<Statement> Block()
        {
            var statements = new List<Statement>();

            while (!Check(TokenType.CloseBrace) && !IsAtEnd())
            {
                statements.Add(Declaration());
            }

            Consume(TokenType.CloseBrace, "Expect '}' after block.");
            return statements;
        }

        private Statement PrintStatement()
        {
            var value = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after a value.");
            return new PrintStatement(value);
        }

        private Statement ExpressionStatement()
        {
            var expression = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after expression,");
            return new ExpressionStatement(expression);
        }
        
        
          // ============================ \\
         //            Helpers             \\
        // ================================ \\ 
        private bool Match(params TokenType[] types)
        {
            foreach (var type in types)
            {
                if (Check(type))
                {
                    Advance();
                    return true;
                }
            }

            return false;
        }

        private bool Check(TokenType type)
        {
            if (IsAtEnd())
            {
                return false;
            }

            return Peek().Type == type;
        }

        private Token Advance()
        {
            if (!IsAtEnd())
            {
                _current++;
            }

            return Previous();
        }

        private bool IsAtEnd()
        {
            return Peek().Type == TokenType.EndOfFile;
        }

        private Token Previous()
        {
            return _tokens[_current - 1];
        }
        
        // Checks the current token without actually advancing the position
        private Token Peek()
        {
            return _tokens[_current];
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }
        
        private ParseError Error(Token token, string message)
        {
            Lux.Error(token, message);
            return new();
        }

        private void Synchronize()
        {
            Advance();

            while (!IsAtEnd())
            {
                if (Previous().Type == TokenType.Semicolon)
                {
                    return;
                }

                switch (Peek().Type)
                {
                    case TokenType.Class:
                    case TokenType.Procedure:
                    case TokenType.Var:
                    case TokenType.For:
                    case TokenType.If:
                    case TokenType.While:
                    case TokenType.Print:
                    case TokenType.Return:
                        return;
                }

                Advance();
            }
        }
    }
}