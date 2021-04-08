using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace Luxembourg
{
    public class Parser
    {
        private class ParseError : Exception { }
        
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
                if (Match(TokenType.Var))
                {
                    return VarDeclaration();
                }

                if (Match(TokenType.Procedure))
                {
                    return Procedure("procedure");
                }

                return Statement();
            }
            catch (ParseError e)
            {
                Synchronize();
                return null;
            }
        }

        private Statement.Function Procedure(string kind)
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
            return new Statement.Var(name, initializer);
        }
        
        
        // Checks the current token without actually advancing the position
        private Token Peek()
        {
            return _tokens[_current];
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
                expression = new Expression.Logical(expression, op, right);
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
                expression = new Expression.Logical(expression, op, right);
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

                if (expression is Expression.Variable)
                {
                    var name = ((Expression.Variable) expression).Name;
                    return new Expression.Assign(name, value);
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
                expression = new Expression.Binary(expression, op, right);
            }

            return expression;
        }

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

        private Expression Comparison()
        {
            var expression = Term();
            while (Match(TokenType.Greater, TokenType.GreaterEqual, TokenType.Less, TokenType.LessEqual))
            {
                var op = Previous();
                var right = Term();
                expression = new Expression.Binary(expression, op, right);
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
                expression = new Expression.Binary(expression, op, right);
            }

            return expression;
        }

        private Expression Factor()
        {
            var expression = Unary();

            while (Match(TokenType.Slash, TokenType.Star))
            {
                var op = Previous();
                var right = Unary();
                expression = new Expression.Binary(expression, op, right);
            }

            return expression;
        }

        private Expression Unary()
        {
            if (Match(TokenType.Bang, TokenType.Minus))
            {
                var op = Previous();
                var right = Unary();
                return new Expression.Unary(op, right);
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
            return new Expression.Call(callee, paren, arguments);
        }

        private Expression Primary()
        {
            if (Match(TokenType.False))
            {
                return new Expression.Literal(false);
            }

            if (Match(TokenType.True))
            {
                return new Expression.Literal(true);
            }

            if (Match(TokenType.Nil))
            {
                return new Expression.Literal(null);
            }

            if (Match(TokenType.Number, TokenType.String))
            {
                return new Expression.Literal(Previous().Literal);
            }

            if (Match(TokenType.OpenParen))
            {
                var expression = Expression();
                Consume(TokenType.CloseParen, "Expect ')' after expression.");
                return new Expression.Grouping(expression);
            }

            if (Match(TokenType.Identifier))
            {
                return new Expression.Variable(Previous());
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
                return new Statement.Block(Block());
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
            return new Statement.Return(keyword, value);
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
                body = new Statement.Block(new() { body, new Statement.Expression(increment) });
            }

            if (condition == null)
            {
                condition = new Expression.Literal(true);
            }

            body = new Statement.While(condition, body);

            if (initializer != null)
            {
                body = new Statement.Block(new() {initializer, body});
            }
            
            return body;
        }

        private Statement WhileStatement()
        {
            Consume(TokenType.OpenParen, "Expect '(' after 'while'.");
            var condition = Expression();
            Consume(TokenType.CloseParen, "Expect ')' after condition.");
            var body = Statement();

            return new Statement.While(condition, body);
        }

        private Statement IfStatement()
        {
            Consume(TokenType.OpenParen, "Expect '(' after 'if'.");
            var condition = Expression();

            var thenBranch = Statement();
            Statement elseBranch = null;
            if (Match(TokenType.Else))
            {
                elseBranch = Statement();
            }

            return new Statement.If(condition, thenBranch, elseBranch);
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
            return new Statement.Print(value);
        }

        private Statement ExpressionStatement()
        {
            var expression = Expression();
            Consume(TokenType.Semicolon, "Expect ';' after expression,");
            return new Statement.Expression(expression);
        }

        private Token Consume(TokenType type, string message)
        {
            if (Check(type))
            {
                return Advance();
            }

            throw Error(Peek(), message);
        }
        
        // Panic Mode \\
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