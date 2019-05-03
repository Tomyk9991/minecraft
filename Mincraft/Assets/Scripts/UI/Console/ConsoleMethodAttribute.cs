using System;

[AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = true)]
sealed class ConsoleMethodAttribute : Attribute
{
    public string stringName;
    public ConsoleMethodAttribute(string method)
    {
        stringName = method;
    }
}
