using System;

namespace Luxembourg
{
    public class ReturnError : Exception
    {
        public ReturnError(object value)
        {
            Value = value;
        }
        
        public object Value { get; }
    }
}