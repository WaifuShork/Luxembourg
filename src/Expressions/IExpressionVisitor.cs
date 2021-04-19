namespace Luxembourg.Expressions
{
    public interface IExpressionVisitor<out T>
    {
        T VisitBinaryExpression(BinaryExpression expression);
        T VisitGroupingExpression(GroupingExpression expression);
        T VisitLiteralExpression(LiteralExpression expression);
        T VisitUnaryExpression(UnaryExpression expression);
        T VisitCallExpression(CallExpression expression);
        T VisitGetExpression(GetExpression expression);
        T VisitLogicalExpression(LogicalExpression expression);
        T VisitSetExpression(SetExpression expression);
        T VisitThisExpression(ThisExpression expression);
        T VisitVariableExpression(VariableExpression expression);
        T VisitAssignExpression(AssignExpression expression);
        T VisitBaseExpression(BaseExpression expression);
    }
}