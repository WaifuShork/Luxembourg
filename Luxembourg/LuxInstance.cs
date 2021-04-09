using System.Collections.Generic;

namespace Luxembourg
{
    public class LuxInstance
    {
        private readonly LuxClass _class;
        private readonly Dictionary<string, object> _fields = new();
        
        public LuxInstance(LuxClass @class)
        {
            _class = @class;
        }

        public object Get(Token name)
        {
            if (_fields.ContainsKey(name.Lexeme))
            {
                return _fields[name.Lexeme];
            }

            var method = _class.FindMethod(name.Lexeme);
            if (method != null)
            {
                return method.Bind(this);
            }

            throw new RuntimeError(name, $"Undefined property '{name.Lexeme}'.");
        }

        public void Set(Token name, object value)
        {
            _fields.Put(name.Lexeme, value);
        }
        
        public override string ToString() => $"{_class.Name} instance";
    }
}