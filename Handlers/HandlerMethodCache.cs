using System;

namespace InteractionsPlus.Handlers
{
    public class HandlerMethodCache
    {
        public readonly string ActionName;
        public readonly Type DelegateType;
        public readonly Type[] ParameterTypes;
        public readonly Delegate Delegate;

        public HandlerMethodCache(string actionName, Type delegateType, Type[] parameterTypes, Delegate @delegate)
        {
            ActionName = actionName;
            DelegateType = delegateType;
            ParameterTypes = parameterTypes;
            Delegate = @delegate;
        }
    }
}