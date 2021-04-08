using System.Text;

namespace Luxembourg
{
    public class AstPrinter : Expression.Visitor<string>, Statement.Visitor<string>
    {
        public string Print(Expression expression)
        {
            return expression.Accept(this);
        }

        public string Print(Statement statement)
        {
            return statement.Accept(this);
        }
        
        private string Parenthesize(string name, params Expression[] expressions)
        {
            var sb = new StringBuilder();

            sb.Append("(").Append(name);
            foreach (var expression in expressions)
            {
                sb.Append(" ");
                sb.Append(expression.Accept(this));
            }

            sb.Append(")");

            return sb.ToString();
        }

        // Expressions \\
        public string VisitBinaryExpression(Expression.Binary expression)
        {
            return Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);
        }

        public string VisitGroupingExpression(Expression.Grouping expression)
        {
            return Parenthesize("group", expression.Expression);
        }

        public string VisitLiteralExpression(Expression.Literal expression)
        {
            if (expression.Value == null)
            {
                return "nil";
            }

            return expression.Value.ToString();
        }

        public string VisitUnaryExpression(Expression.Unary expression)
        {
            return Parenthesize(expression.Operator.Lexeme, expression.Right);
        }

        public string VisitCallExpression(Expression.Call expression)
        {
            throw new System.NotImplementedException();
        }

        public string VisitGetExpression(Expression.Get expression)
        {
            throw new System.NotImplementedException();
        }

        public string VisitLogicalExpression(Expression.Logical expression)
        {
            throw new System.NotImplementedException();
        }

        public string VisitSetExpression(Expression.Set expression)
        {
            throw new System.NotImplementedException();
        }

        public string VisitBaseExpression(Expression.Base expression)
        {
            throw new System.NotImplementedException();
        }

        public string VisitThisExpression(Expression.This expression)
        {
            throw new System.NotImplementedException();
        }

        public string VisitVariableExpression(Expression.Variable expression)
        {
            throw new System.NotImplementedException();
        }

        public string VisitAssignExpression(Expression.Assign expression)
        {
            throw new System.NotImplementedException();
        }

        
        // Statements \\
        public string VisitBlockStatement(Statement.Block statement)
        {
            var builder = new StringBuilder();
            builder.Append("(block ");

            foreach (var state in statement.Statements)
            {
                builder.Append(state.Accept(this));
            }

            builder.Append(")");
            return builder.ToString();
        }

        public string VisitClassStatement(Statement.Class statement)
        {
            var builder = new StringBuilder();
            builder.Append($"(class {statement.Name.Lexeme}");

            if (statement.BaseClass != null)
            {
                builder.Append($" < {Print(statement.BaseClass)}");
            }

            foreach (var method in statement.Methods)
            {
                builder.Append($"< {Print(method)}");
            }

            builder.Append(")");
            return builder.ToString();
        }

        public string VisitExpressionStatement(Statement.Expression statement)
        {
            return Parenthesize(";", statement.ExpressionSt);
        }

        public string VisitFunctionStatement(Statement.Function statement)
        {
            var builder = new StringBuilder();
            builder.Append($"(procedure {statement.Name.Lexeme} (");

            foreach (var param in statement.Parameters)
            {
                if (param != statement.Parameters[0])
                {
                    builder.Append(" ");
                }

                builder.Append(param.Lexeme);
            }

            builder.Append(") ");
            foreach (var body in statement.Body)
            {
                builder.Append(body.Accept(this));
            }

            builder.Append(")");
            return builder.ToString();
        }

        public string VisitIfStatement(Statement.If statement)
        {
            throw new System.NotImplementedException();
        }

        public string VisitReturnStatement(Statement.Return statement)
        {
            throw new System.NotImplementedException();
        }

        public string VisitPrintStatement(Statement.Print statement)
        {
            throw new System.NotImplementedException();
        }

        public string VisitVarStatement(Statement.Var statement)
        {
            throw new System.NotImplementedException();
        }

        public string VisitWhileStatement(Statement.While statement)
        {
            throw new System.NotImplementedException();
        }
    }
}