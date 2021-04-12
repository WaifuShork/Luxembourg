using System.Collections.Generic;

namespace Luxembourg
{
    public class LuxFunction : ILuxCallable
    {
        private readonly FunctionStatement _declaration;
        private readonly Environment _closure;
        private readonly bool _isInitializer;
        
        public LuxFunction(FunctionStatement declaration, Environment closure, bool isInitializer)
        {
            _declaration = declaration;
            _closure = closure;
            _isInitializer = isInitializer;
        }

        public int Arity()
        {
            return _declaration.Parameters.Count;
        }

        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var environment = new Environment(_closure);

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
                if (_isInitializer)
                {
                    return _closure.GetAt(0, "this");
                }
                
                return r.Value;
            }

            if (_isInitializer)
            {
                return _closure.GetAt(0, "this");
            }
            
            return null;
        }

        public LuxFunction Bind(LuxInstance instance)
        {
            var environment = new Environment(_closure);
            environment.Define("this", instance);
            return new(_declaration, environment, _isInitializer);
        }
    }
}