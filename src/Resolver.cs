using System.Collections.Generic;
using System.Linq;

namespace Luxembourg
{
    public class Resolver : IExpressionVisitor<object>, IStatementVisitor<object>
    {
        private readonly Interpreter _interpreter;
        private readonly Stack<Dictionary<string, bool>> _scopes = new();
        private FunctionType _currentFunction = FunctionType.None;
        private ClassType _currentClass = ClassType.None;

        public Resolver(Interpreter interpreter)
        {
            _interpreter = interpreter;
        }
        
        public object VisitBinaryExpression(BinaryExpression expression)
        {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return null;
        }

        public object VisitGroupingExpression(GroupingExpression expression)
        {
            Resolve(expression.Expression);
            return null;
        }

        public object VisitLiteralExpression(LiteralExpression expression)
        {
            return null;
        }

        public object VisitUnaryExpression(UnaryExpression expression)
        {
            Resolve(expression.Right);
            return null;
        }

        public object VisitCallExpression(CallExpression expression)
        {
            Resolve(expression.Callee);

            foreach (var argument in expression.Arguments)
            {
                Resolve(argument);
            }
            return null;
        }

        public object VisitGetExpression(GetExpression expression)
        {
            Resolve(expression.Object);
            return null;
        }

        public object VisitLogicalExpression(LogicalExpression expression)
        {
            Resolve(expression.Left);
            Resolve(expression.Right);
            return null;
        }

        public object VisitSetExpression(SetExpression expression)
        {
            Resolve(expression.Value);
            Resolve(expression.Object);
            return null;
        }

        public object VisitBaseExpression(BaseExpression expression)
        {
            throw new System.NotImplementedException();
        }

        public object VisitThisExpression(ThisExpression expression)
        {
            if (_currentClass == ClassType.None)
            {
                Lux.Error(expression.Keyword, "Can't use 'this' outside of a class.");
                return null;
            }
            
            ResolveLocal(expression, expression.Keyword);
            return null;
        }

        public object VisitVariableExpression(VariableExpression expression)
        {
            if (_scopes.Any() && _scopes.Peek()[expression.Name.Lexeme] == false)
            {
                Lux.Error(expression.Name, "Can't read local variable in its own initializer.");
            }

            ResolveLocal(expression, expression.Name);
            return null;
        }

        public object VisitAssignExpression(AssignExpression expression)
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

        private void ResolveFunction(FunctionStatement function, FunctionType type)
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
        
        public object VisitBlockStatement(BlockStatement statement)
        {
            BeginScope();
            Resolve(statement.Statements);
            EndScope();
            return null;
        }

        public object VisitClassStatement(ClassStatement statement)
        {
            var enclosingClass = _currentClass;
            _currentClass = ClassType.Class;
            
            Declare(statement.Name);
            Define(statement.Name);

            BeginScope();
            _scopes.Peek().Put("this", true);
            
            foreach (var method in statement.Methods)
            {
                var declaration = FunctionType.Method;
                if (method.Name.Lexeme.Equals("init"))
                {
                    declaration = FunctionType.Initializer;
                }
                
                ResolveFunction(method, declaration);
            }
            EndScope();
            _currentClass = enclosingClass;
            return null;
        }

        public object VisitExpressionStatement(ExpressionStatement statement)
        {
            Resolve(statement.Expression);
            return null;
        }

        public object VisitFunctionStatement(FunctionStatement statement)
        {
            Declare(statement.Name);
            Define(statement.Name);
            
            ResolveFunction(statement, FunctionType.Function);
            return null;
        }

        public object VisitIfStatement(IfStatement statement)
        {
            Resolve(statement.Condition);
            Resolve(statement.ThenBranch);
            if (statement.ElseBranch != null)
            {
                Resolve(statement.ElseBranch);
            }

            return null;
        }

        public object VisitReturnStatement(ReturnStatement statement)
        {
            if (_currentFunction == FunctionType.None)
            {
                Lux.Error(statement.Keyword, "Can't return from top-level code.");
            }
            
            if (statement.Value != null)
            {
                if (_currentFunction == FunctionType.Initializer)
                {
                    Lux.Error(statement.Keyword, "Can't return a value from an initializer");
                }
            
                Resolve(statement.Value);
            }
            
            return null;
        }

        public object VisitPrintStatement(PrintStatement statement)
        {
            Resolve(statement.Expression);
            return null;
        }

        public object VisitVarStatement(VarStatement statement)
        {
            Declare(statement.Name);
            if (statement.Initializer != null)
            {
                Resolve(statement.Initializer);
            }

            Define(statement.Name);
            return null;
        }

        public object VisitWhileStatement(WhileStatement statement)
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