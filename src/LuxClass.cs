using System.Collections.Generic;

namespace Luxembourg
{
    public class LuxClass : ILuxCallable
    {
        private readonly Dictionary<string, LuxFunction> _methods;
        
        public LuxClass(string name, Dictionary<string, LuxFunction> methods)
        {
            Name = name;
            _methods = methods;
        }

        public string Name { get; }

        public LuxFunction FindMethod(string name)
        {
            if (_methods.ContainsKey(name))
            {
                return _methods.Get(name); //[name];
            }

            return null;
        }
        
        public object Call(Interpreter interpreter, List<object> arguments)
        {
            var instance = new LuxInstance(this);

            var initializer = FindMethod("init");
            if (initializer != null)
            {
                initializer.Bind(instance).Call(interpreter, arguments);
            }
            
            return instance;
        }

        public int Arity()
        {
            var initializer = FindMethod("init");
            if (initializer == null)
            {
                return 0;
            }

            return initializer.Arity();
        }

        public override string ToString() => Name;
    }
}