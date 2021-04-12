using System;
using System.Collections.Generic;
using System.Text;

namespace Luxembourg
{
    public class AstPrinter : IExpressionVisitor<string>, IStatementVisitor<string>
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

            sb.Append('(').Append(name);
            foreach (var expression in expressions)
            {
                sb.Append(' ');
                sb.Append(expression.Accept(this));
            }

            sb.Append(')');

            return sb.ToString();
        }

        private string Parenthesize2(string name, params object[] parts)
        {
            var builder = new StringBuilder();

            builder.Append('(').Append(name);
            Transform(builder, parts);
            builder.Append(')');

            return builder.ToString();
        }

        private void Transform(StringBuilder builder, params object[] parts)
        {
            foreach (var part in parts)
            {
                builder.Append(' ');

                if (part is Expression)
                {
                    builder.Append(((Expression) part).Accept(this));
                }
                else if (part is Statement)
                {
                    builder.Append(((Statement) part).Accept(this));
                }
                else if (part is Token)
                {
                    builder.Append(((Token) part).Lexeme);
                }
                else if (part is Array)
                {
                    Transform(builder, parts);
                }
                else
                {
                    builder.Append(part);
                }
            }
        }

        // Expressions \\
        public string VisitBinaryExpression(BinaryExpression expression)
        {
            return Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);
        }

        public string VisitGroupingExpression(GroupingExpression expression)
        {
            return Parenthesize("group", expression.Expression);
        }

        public string VisitLiteralExpression(LiteralExpression expression)
        {
            if (expression.Value == null)
            {
                return "nil";
            }

            return expression.Value.ToString();
        }

        public string VisitUnaryExpression(UnaryExpression expression)
        {
            return Parenthesize(expression.Operator.Lexeme, expression.Right);
        }

        public string VisitCallExpression(CallExpression expression)
        {
            return Parenthesize2("call", expression.Callee, expression.Arguments);
        }

        public string VisitGetExpression(GetExpression expression)
        {
            return Parenthesize2(".", expression.Object, expression.Name.Lexeme);
        }

        public string VisitLogicalExpression(LogicalExpression expression)
        {
            return Parenthesize(expression.Operator.Lexeme, expression.Left, expression.Right);
        }

        public string VisitSetExpression(SetExpression expression)
        {
            return Parenthesize2("=", expression.Object, expression.Name.Lexeme, expression.Value);
        }

        public string VisitBaseExpression(BaseExpression expression)
        {
            throw new NotImplementedException();
        }

        public string VisitThisExpression(ThisExpression expression)
        {
            return "this";
        }

        public string VisitVariableExpression(VariableExpression expression)
        {
            return expression.Name.Lexeme;
        }

        public string VisitAssignExpression(AssignExpression expression)
        {
            return Parenthesize2("=", expression.Name.Lexeme, expression.Value);
        }

        
        // Statements \\
        public string VisitBlockStatement(BlockStatement statement)
        {
            var builder = new StringBuilder();
            builder.Append("(block ");

            foreach (var state in statement.Statements)
            {
                builder.Append(state.Accept(this));
            }

            builder.Append(')');
            return builder.ToString();
        }

        public string VisitClassStatement(ClassStatement statement)
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

            builder.Append(')');
            return builder.ToString();
        }

        public string VisitExpressionStatement(ExpressionStatement statement)
        {
            return Parenthesize(";", statement.Expression);
        }

        public string VisitFunctionStatement(FunctionStatement statement)
        {
            var builder = new StringBuilder();
            builder.Append($"(procedure {statement.Name.Lexeme} (");

            foreach (var param in statement.Parameters)
            {
                if (param != statement.Parameters[0])
                {
                    builder.Append(' ');
                }

                builder.Append(param.Lexeme);
            }

            builder.Append(") ");
            foreach (var body in statement.Body)
            {
                builder.Append(body.Accept(this));
            }

            builder.Append(')');
            return builder.ToString();
        }

        public string VisitIfStatement(IfStatement statement)
        {
            if (statement.ElseBranch == null)
            {
                return Parenthesize2("if", statement.Condition, statement.ThenBranch);
            }

            return Parenthesize2("if-else", statement.Condition, statement.ThenBranch, statement.ElseBranch);
        }

        public string VisitReturnStatement(ReturnStatement statement)
        {
            if (statement.Value == null)
            {
                return "(return)";
            }

            return Parenthesize("return", statement.Value);
        }

        public string VisitPrintStatement(PrintStatement statement)
        {
            return Parenthesize("print", statement.Expression);
        }

        public string VisitVarStatement(VarStatement statement)
        {
            if (statement.Initializer == null)
            {
                return Parenthesize2("var", statement.Name);
            }

            return Parenthesize2("var", statement.Name, "=", statement.Initializer);
        }

        public string VisitWhileStatement(WhileStatement statement)
        {
            return Parenthesize2("while", statement.Condition, statement.Body);
        }
    }
}