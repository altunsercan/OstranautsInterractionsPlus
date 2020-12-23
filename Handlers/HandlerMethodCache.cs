using System;

namespace InteractionsPlus.Handlers
{
    public class HandlerMethodCache
    {
        public readonly string ActionName;
        public readonly Type[] ParameterTypes;
        public readonly Func<object[],bool> Delegate;

        public HandlerMethodCache(string actionName, Type[] parameterTypes, Func<object[],bool> @delegate)
        {
            ActionName = actionName;
            ParameterTypes = parameterTypes;
            Delegate = @delegate;
        }
    }
}