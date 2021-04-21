using System.Collections.Generic;

namespace Luxembourg
{
    public class LuxClass : ILuxCallable
    {
        private readonly Dictionary<string, LuxFunction> _methods;
        
        public LuxClass(string name, Dictionary<string, LuxFunction> methods, LuxClass baseClass)
        {
            Name = name;
            BaseClass = baseClass;
            
            _methods = methods;
        }

        public string Name { get; }
        public LuxClass BaseClass { get; }

        public LuxFunction FindMethod(string name)
        {
            if (_methods.ContainsKey(name))
            {
                return _methods.Get(name); 
            }

            if (BaseClass != null)
            {
                return BaseClass.FindMethod(name);
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