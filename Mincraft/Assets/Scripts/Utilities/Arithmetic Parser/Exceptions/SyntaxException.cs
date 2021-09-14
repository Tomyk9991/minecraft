using System;

namespace ArithmeticParser.Exceptions
{
    public class SyntaxException : Exception
    {
        public SyntaxException(string message) : base(message)
        {
            
        }
    }
}