using System.Collections.Generic;

namespace Luxembourg
{
    public class LuxFunction : ILuxCallable
    {
        private readonly Statement.Function _declaration;
        
        public LuxFunction(Statement.Function declaration)
        {
            _declaration = declaration;
        }

        public int Arity()
        {
            return _declaration.Parameters.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(interpreter.Environment);
            for (var i = 0; i < _declaration.Parameters.Count; i++)
            {
                environment.Define(_declaration.Parameters[i].Lexeme, arguments[i]);
            }

            try
            {
                interpreter.ExecuteBlock(_declaration.Body, environment);
            }
            catch (Return r)
            {
                return r.Value;
            }
            
            return null;
        }
    }
}