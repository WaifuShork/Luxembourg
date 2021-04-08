using System;
using System.Collections.Generic;

namespace Luxembourg
{
    public class Interpreter : Expression.Visitor<object>, Statement.Visitor<object>
    {
        public Environment Environment = new();

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
            var right = Evaluate(expression.Right);

            switch (expression.Operator.Type)
            {
                case TokenType.Minus:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left - (double) right;
                case TokenType.Slash:
                    CheckNumberOperands(expression.Operator, left, right);
                    return (double) left / (double) right;
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

            if (callee is not ILuxCallable)
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
            throw new NotImplementedException();
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
            throw new NotImplementedException();
        }

        public object VisitBaseExpression(Expression.Base expression)
        {
            throw new NotImplementedException();
        }

        public object VisitThisExpression(Expression.This expression)
        {
            throw new NotImplementedException();
        }

        public object VisitVariableExpression(Expression.Variable expression)
        {
            return Environment.Get(expression.Name);
        }

        public object VisitAssignExpression(Expression.Assign expression)
        {
            var value = Evaluate(expression.Value);
            Environment.Assign(expression.Name, value);
            return value;
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
            var previous = Environment;
            try
            {
                Environment = environment;

                foreach (var statement in statements)
                {
                    Execute(statement);
                }
            }
            finally
            {
                Environment = previous;
            }
        }
        
        public object VisitBlockStatement(Statement.Block statement)
        {
            ExecuteBlock(statement.Statements, new(Environment));
            return null;
        }

        public object VisitClassStatement(Statement.Class statement)
        {
            throw new NotImplementedException();
        }

        public object VisitExpressionStatement(Statement.Expression statement)
        {
            Evaluate(statement.ExpressionSt);
            return null;
        }

        public object VisitFunctionStatement(Statement.Function statement)
        {
            var function = new LuxFunction(statement);
            Environment.Define(statement.Name.Lexeme, function);
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
            
            Environment.Define(statement.Name.Lexeme, value);
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