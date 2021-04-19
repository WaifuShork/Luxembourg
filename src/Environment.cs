using System.Collections.Generic;

namespace Luxembourg
{
    public class Environment
    {
        private readonly Dictionary<string, object> _values = new();

        public Environment()
        {
            Enclosing = null;
        }

        public Environment(Environment enclosing)
        {
            Enclosing = enclosing;
        }
        
        public Environment Enclosing { get; }

        public void Define(string name, object value)
        {
            _values.Add(name, value);
        }

        public object Get(Token name)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                // Modify the value on the spot instead of adding a new key
                return _values.Get(name.Lexeme);
            }

            if (Enclosing != null)
            {
                return Enclosing.Get(name);
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public void Assign(Token name, object value)
        {
            if (_values.ContainsKey(name.Lexeme))
            {
                // Modify the value on the spot instead of adding a new key
                _values.Put(name.Lexeme, value); //[name.Lexeme] = value;
                return;
            }

            if (Enclosing != null)
            {
                Enclosing.Assign(name, value);
                return;
            }

            throw new RuntimeError(name, $"Undefined variable '{name.Lexeme}'.");
        }

        public object GetAt(int distance, string name)
        {
            return Ancestor(distance)._values.Get(name); // [name];
        }

        public Environment Ancestor(int distance)
        {
            var environment = this;
            for (var i = 0; i < distance; i++)
            {
                environment = environment.Enclosing;
            }

            return environment;
        }

        public void AssignAt(int distance, Token name, object value)
        {
            Ancestor(distance)._values[name.Lexeme] = value;
        }

        public override string ToString()
        {
            var result = _values.ToString();
            if (Enclosing != null)
            {
                result += $" -> {Enclosing}";
            }

            return result;
        }
    }
}