using System;
using System.Collections.Generic;
using System.Reflection.Metadata;

namespace Luxembourg
{
    public class Interpreter : Expression.Visitor<object>, Statement.Visitor<object>
    {
        private Environment _environment { get; set; } = Globals;
        
        private readonly Dictionary<Expression, int> _locals = new();
        
        public static readonly Environment Globals = new();

        public void Interpret(Expression expression)
        {
            try
            {
                var value = Evaluate(expression);
                Console.Out.WriteLine(Stringify(value));
            }
            catch (RuntimeError e)
            {
                Lux.RuntimeError(e);
            }
        }
        
        public void Interpret(List<Statement> statements)
        {
            try
            {
                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            catch (RuntimeError e)
            {
                Lux.RuntimeError(e);
            }
        }

        public void Resolve(Expression expression, int depth)
        {
            _locals[expression] = depth;
        }

        private string Stringify(object obj)
        {
            if (obj == null)
            {
                return "nil";
            }

            if (obj is double)
            {
                var text = obj.ToString();
                if (text.EndsWith(".0"))
                {
                    text = text.SubstringEx(0, text.Length - 2);
                }

                return text;
            }

            return obj.ToString();
        }
        
        public object VisitBinaryExpression(Expression.Binary expression)
        {
            var left = Evaluate(expression.Left);
            var operatorType = expression.Operator.Type;
            var right = Evaluate(expression.Right);
            
            
            switch (expression.Operator.Type)
            {
                case TokenType.Minus:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left - (double) right;
                case TokenType.Slash:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left / (double) right;
                
                case TokenType.StarStar:
                    CheckNumberOperands(expression.Operator, left, right);
                    return Math.Pow((double) left, (double) right);
                    
                case TokenType.Star:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left * (double) right;

                case TokenType.Plus:
                    if (left is double && right is double)
                    {
                        return (double) left + (double) right;
                    }

                    if (left is string && right is string)
                    {
                        return (string) left + (string) right;
                    }

                    if (left is string && right is double)
                    {
                        return (string) left + (double) right;
                    }

                    if (left is double && right is string)
                    {
                        return (double) left + (string) right;
                    }

                    throw new RuntimeError(expression.Operator, "Operands must be two numbers or two strings");
                
                case TokenType.BangEquals:
                    return !IsEqual(left, right);
                case TokenType.EqualEqual:
                    return IsEqual(left, right);
                
                case TokenType.Greater:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left > (double) right;
                case TokenType.GreaterEqual:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left >= (double) right;
                case TokenType.Less:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left < (double) right;
                case TokenType.LessEqual:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left <= (double) right;
            }

            // Unreachable
            return null;
        }

        public object VisitGroupingExpression(Expression.Grouping expression)
        {
            return Evaluate(expression.Expression);
        }

        public object VisitLiteralExpression(Expression.Literal expression)
        {
            return expression.Value;
        }

        public object VisitUnaryExpression(Expression.Unary expression)
        {
            var right = Evaluate(expression.Right);

            switch (expression.Operator.Type)
            {
                case TokenType.Bang:
                    return !IsTruthy(right);
                case TokenType.Minus:
                    CheckNumberOperand(expression.Operator, right);
                    return -(double) right;
            }

            // Unreachable
            return null;
        }

        public object VisitCallExpression(Expression.Call expression)
        {
            var callee = Evaluate(expression.Callee);
            var arguments = new List<object>();

            foreach (var argument in expression.Arguments)
            {
                arguments.Add(Evaluate(argument));
            }

            if (callee is not LuxFunction)
            {
                throw new RuntimeError(expression.Paren, "Can only call functions and classes.");
            }
            
            var function = (LuxFunction) callee;
            if (arguments.Count != function.Arity())
            {
                throw new RuntimeError(expression.Paren, $"Expected {function.Arity()} arguments but got {arguments.Count}.");
            }

            return function.Call(this, arguments);
        }

        public object VisitGetExpression(Expression.Get expression)
        {
            var obj = Evaluate(expression.Object);

            if (obj is LuxInstance li)
            {
                return li.Get(expression.Name);
            }

            throw new RuntimeError(expression.Name, "Only instances have properties.");
        }

        public object VisitLogicalExpression(Expression.Logical expression)
        {
            var left = Evaluate(expression.Left);

            if (expression.Operator.Type == TokenType.Or)
            {
                if (IsTruthy(left))
                {
                    return left;
                }
            }
            else
            {
                if (!IsTruthy(left))
                {
                    return left;
                }
            }

            return Evaluate(expression.Right);
        }

        public object VisitSetExpression(Expression.Set expression)
        {
            var obj = Evaluate(expression.Object);

            if (obj is not LuxInstance instance)
            {
                throw new RuntimeError(expression.Name, "Only instances have fields");
            }

            var value = Evaluate(expression.Value);
            instance.Set(expression.Name, value);
            return value;
        }

        public object VisitBaseExpression(Expression.Base expression)
        {
            throw new NotImplementedException();
        }

        public object VisitThisExpression(Expression.This expression)
        {
            return LookupVariable(expression.Keyword, expression);
        }

        public object VisitVariableExpression(Expression.Variable expression)
        {
            return LookupVariable(expression.Name, expression);
        }

        public object VisitAssignExpression(Expression.Assign expression)
        {
            var value = Evaluate(expression.Value);
            var distance = _locals[expression];
            
            if (!distance.Equals(null))
            {
                _environment.AssignAt(distance, expression.Name, value);
            }
            else
            {
                Globals.Assign(expression.Name, value);
            }

            return value;
        }

        private object LookupVariable(Token name, Expression expression)
        {
            var distance = _locals[expression];

            if (!distance.Equals(null))
            {
                return _environment.GetAt(distance, name.Lexeme);
            }
            else
            {
                return Globals.Get(name);
            }
        }

        private bool IsTruthy(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (obj is bool)
            {
                return (bool) obj;
            }

            return true;
        }

        private void CheckNumberOperand(Token op, object operand)
        {
            if (operand is double)
            {
                return;
            }

            throw new RuntimeError(op, "The operand must be a number");
        }

        private void CheckNumberOperands(Token op, object left, object right)
        {
            if (left is double && right is double)
            {
                return;
            }

            throw new RuntimeError(op, "Operands must be numbers.");
        }
        
        private bool IsEqual(object left, object right)
        {
            if (left == null && right == null)
            {
                return true;
            }

            if (left == null)
            {
                return false;
            }

            return left.Equals(right);
        }
        
        private object Evaluate(Expression expression)
        {
            return expression.Accept(this);
        }

        public void ExecuteBlock(List<Statement> statements, Environment environment)
        {
            var previous = _environment;
            try
            {
                _environment = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                _environment = previous;
            }
        }
        
        public object VisitBlockStatement(Statement.Block statement)
        {
            ExecuteBlock(statement.Statements, new(_environment));
            return null;
        }

        public object VisitClassStatement(Statement.Class statement)
        {
            _environment.Define(statement.Name.Lexeme, null);

            var methods = new Dictionary<string, LuxFunction>();
            foreach (var method in statement.Methods)
            {
                var function = new LuxFunction(method, _environment, method.Name.Lexeme.Equals("init"));
                methods.Put(method.Name.Lexeme, function);
            }
            
            var @class = new LuxClass(statement.Name.Lexeme, methods);
            _environment.Assign(statement.Name, @class);
            return null;
        }

        public object VisitExpressionStatement(Statement.Expression statement)
        {
            Evaluate(statement.ExpressionSt);
            return null;
        }

        public object VisitFunctionStatement(Statement.Function statement)
        {
            var function = new LuxFunction(statement, _environment, false);
            _environment.Define(statement.Name.Lexeme, function);
            return null;
        }

        public object VisitIfStatement(Statement.If statement)
        {
            if (IsTruthy(Evaluate(statement.Condition)))
            {
                Execute(statement.ThenBranch);
            }
            else if (statement.ElseBranch != null)
            {
                Execute(statement.ElseBranch);
            }

            return null;
        }

        public object VisitReturnStatement(Statement.Return statement)
        {
            object value = null;
            if (statement.Value != null)
            {
                value = Evaluate(statement.Value);
            }

            throw new Return(value);
        }

        public object VisitPrintStatement(Statement.Print statement)
        {
            var value = Evaluate(statement.Expression);
            Console.Out.WriteLine(Stringify(value));
            return null;
        }

        public object VisitVarStatement(Statement.Var statement)
        {
            object value = null;

            if (statement.Initializer != null)
            {
                value = Evaluate(statement.Initializer);
            }
            
            _environment.Define(statement.Name.Lexeme, value);
            return null;
        }

        public object VisitWhileStatement(Statement.While statement)
        {
            while (IsTruthy(Evaluate(statement.Condition)))
            {
                Execute(statement.Body);
            }

            return null;
        }

        private void Execute(Statement statement)
        {
            statement.Accept(this);
        }
    }
}