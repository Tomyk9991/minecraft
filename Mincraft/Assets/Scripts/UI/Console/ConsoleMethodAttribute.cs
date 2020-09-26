using System;

namespace Core.UI.Console
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
    sealed class ConsoleMethodAttribute : Attribute
    {
        public string stringName;
        public string description;
        
        public ConsoleMethodAttribute(string methodName)
        {
            this.stringName = methodName;
        }
        
        public ConsoleMethodAttribute(string methodName, string description)
        {
            stringName = methodName;
            this.description = description;
        }
    }
}
