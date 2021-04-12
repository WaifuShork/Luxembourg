namespace Luxembourg
{
    
        public interface IStatementVisitor<out T>
        {
            T VisitBlockStatement(BlockStatement statement);
            T VisitClassStatement(ClassStatement statement);
            T VisitExpressionStatement(ExpressionStatement statement);
            T VisitFunctionStatement(FunctionStatement statement);
            T VisitIfStatement(IfStatement statement);
            T VisitReturnStatement(ReturnStatement statement);
            T VisitPrintStatement(PrintStatement statement);
            T VisitVarStatement(VarStatement statement);
            T VisitWhileStatement(WhileStatement statement);
        }
    
}