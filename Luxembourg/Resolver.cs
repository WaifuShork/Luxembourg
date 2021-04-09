using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata;

namespace Luxembourg
{
    public class Resolver : Expression.Visitor<object>, Statement.Visitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new();
        private FunctionType _currentFunction = FunctionType.None;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }
        
        public object VisitBinaryExpression(Expression.Binary expression)
        {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return null;
        }

        public object VisitGroupingExpression(Expression.Grouping expression)
        {
            Resolve(expression.Expression);
            return null;
        }

        public object VisitLiteralExpression(Expression.Literal expression)
        {
            return null;
        }

        public object VisitUnaryExpression(Expression.Unary expression)
        {
            Resolve(expression.Right);
            return null;
        }

        public object VisitCallExpression(Expression.Call expression)
        {
            Resolve(expression.Callee);

            foreach (var argument in expression.Arguments)
            {
                Resolve(argument);
            }
            return null;
        }

        public object VisitGetExpression(Expression.Get expression)
        {
            throw new System.NotImplementedException();
        }

        public object VisitLogicalExpression(Expression.Logical expression)
        {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return null;
        }

        public object VisitSetExpression(Expression.Set expression)
        {
            throw new System.NotImplementedException();
        }

        public object VisitBaseExpression(Expression.Base expression)
        {
            throw new System.NotImplementedException();
        }

        public object VisitThisExpression(Expression.This expression)
        {
            throw new System.NotImplementedException();
        }

        public object VisitVariableExpression(Expression.Variable expression)
        {
            if (_scopes.Any() && _scopes.Peek()[expression.Name.Lexeme] == false)
            {
                Lux.Error(expression.Name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expression, expression.Name);
            return null;
        }

        public object VisitAssignExpression(Expression.Assign expression)
        {
            Resolve(expression.Value);
            ResolveLocal(expression, expression.Name);
            return null;
        }




        private void ResolveLocal(Expression expression, Token name)
        {
            for (var i = _scopes.Count - 1; i >= 0; i++)
            {
                if (_scopes.ElementAt(i).ContainsKey(name.Lexeme))
                {
                    _interpreter.Resolve(expression, _scopes.Count - 1 - i);
                    return;
                }
            }
        }

        public void Resolve(List<Statement> statements)
        {
            foreach (var statement in statements)
            {
                Resolve(statement);
            }
        }

        public void Resolve(Statement statement)
        {
            statement.Accept(this);
        }

        private void Resolve(Expression expression)
        {
            expression.Accept(this);
        }

        private void BeginScope()
        {
            _scopes.Push(new());
        }

        private void EndScope()
        {
            _scopes.Pop();
        }

        private void ResolveFunction(Statement.Function function, FunctionType type)
        {
            var enclosingType = _currentFunction;
            _currentFunction = type;
            BeginScope();

            foreach (var param in function.Parameters)
            {
                Declare(param);
                Define(param);
            }
            Resolve(function.Body);
            EndScope();
            _currentFunction = enclosingType;
        }
        
        public object VisitBlockStatement(Statement.Block statement)
        {
            BeginScope();
            Resolve(statement.Statements);
            EndScope();
            return null;
        }

        public object VisitClassStatement(Statement.Class statement)
        {
            throw new System.NotImplementedException();
        }

        public object VisitExpressionStatement(Statement.Expression statement)
        {
            Resolve(statement.ExpressionSt);
            return null;
        }

        public object VisitFunctionStatement(Statement.Function statement)
        {
            Declare(statement.Name);
            Define(statement.Name);
            
            ResolveFunction(statement, FunctionType.Function);
            return null;
        }

        public object VisitIfStatement(Statement.If statement)
        {
            Resolve(statement.Condition);
            Resolve(statement.ThenBranch);
            if (statement.ElseBranch != null)
            {
                Resolve(statement.ElseBranch);
            }

            return null;
        }

        public object VisitReturnStatement(Statement.Return statement)
        {
            if (_currentFunction == FunctionType.None)
            {
                Lux.Error(statement.Keyword, "Can't return from top-level code.");
            }
            
            if (statement.Value != null)
            {
                Resolve(statement.Value);
            }

            return null;
        }

        public object VisitPrintStatement(Statement.Print statement)
        {
            Resolve(statement.Expression);
            return null;
        }

        public object VisitVarStatement(Statement.Var statement)
        {
            Declare(statement.Name);
            if (statement.Initializer != null)
            {
                Resolve(statement.Initializer);
            }

            Define(statement.Name);
            return null;
        }

        public object VisitWhileStatement(Statement.While statement)
        {
            Resolve(statement.Condition);
            Resolve(statement.Body);
            return null;
        }

        private void Declare(Token name)
        {
            if (!_scopes.Any())
            {
                return;
            }

            var scope = _scopes.Peek();

            if (scope.ContainsKey(name.Lexeme))
            {
                Lux.Error(name, "Already variable declared with this name in this scope.");
            }
            
            scope[name.Lexeme] = false;
        }

        private void Define(Token name)
        {
            if (!_scopes.Any())
            {
                return;
            }

            _scopes.Peek()[name.Lexeme] = true;
        }
    }
}