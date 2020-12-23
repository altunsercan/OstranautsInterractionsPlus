using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using InteractionsPlus.JetBrains.Annotations;

namespace InteractionsPlus.Handlers
{
    public class HandlerManager
    {
        [NotNull]
        private readonly ILogger logger;

        [NotNull]
        private readonly Dictionary<string, HandlerMethodCache> handlers;

        internal HandlerManager([NotNull] ILogger logger)
        {
            this.logger = logger;
            handlers = new Dictionary<string, HandlerMethodCache>();
        }
        
        public void DisoverHandlersInAllAssemblies()
        {
            var assemblyArray = AppDomain.CurrentDomain.GetAssemblies();
            foreach (Assembly assembly in assemblyArray)
            {
                DiscoverHandlersInAssembly(assembly);
            }
        }
        
        public void DiscoverHandlersInAssembly(Assembly assembly)
        {
            if (assembly == null)
            {
                return;
            }
            
            var typeArray = assembly.GetTypes();
            foreach (Type type in typeArray)
            {
                var methods = type.GetMethods();
                foreach (MethodInfo methodInfo in methods)
                {
                    if (!HasHandlerAttribute(methodInfo))
                    {
                        continue;
                    }

                    var actionName = methodInfo.Name;
                    if (handlers.ContainsKey(actionName))
                    {
                        logger.Warning($"Interaction handler for {actionName} is defined twice. Skipping {methodInfo.FullDescription()}");
                        continue;
                    }
                    
                    var cache = CreateHandlerMethodDelegate(type, methodInfo);
                    handlers.Add(actionName, cache);
                    logger.Log($"Added Handler: {methodInfo.FullDescription()}");
                }
            }
            
        }

        [Pure]
        private HandlerMethodCache CreateHandlerMethodDelegate([NotNull]Type type, [NotNull]MethodInfo methodInfo)
        {
            if (!methodInfo.IsStatic)
            {
                // TODO: Non-static handlers need instance to invoke
                return null;
            }

            ParameterInfo[] parameters = methodInfo.GetParameters();
            Type[] parameterTypes = parameters.Select(p => p.ParameterType).ToArray();

            Type delegateType = typeof(Action<>).MakeGenericType(parameterTypes);
            Delegate delegateMethod = Delegate.CreateDelegate(delegateType, methodInfo);
            return new HandlerMethodCache(methodInfo.Name, delegateType, parameterTypes, delegateMethod);
        }

        private bool HasHandlerAttribute(MethodInfo method) =>
            method.GetCustomAttributes(typeof(InteractionHandlerAttribute), true).Length>0;


        public bool TryGetHandlerViaName(string name, out HandlerMethodCache cache) =>
            handlers.TryGetValue(name, out cache);
    }
}