using System;

namespace RevitAI.Attributes
{
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public class RevitTaskActionAttribute : Attribute
    {
        public string ActionName { get; }

        public RevitTaskActionAttribute(string actionName)
        {
            ActionName = actionName;
        }
    }
}
