using System.Collections.Generic;

namespace Luxembourg
{
    public interface ILuxCallable
    {
        public int Arity();

        public object Call(Interpreter interpreter, List<object> arguments);
    }
}