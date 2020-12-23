using System;
using System.Collections.Generic;
using InteractionsPlus.JetBrains.Annotations;

namespace InteractionsPlus
{
    public class ServiceLocator
    {
        [NotNull]
        private readonly Dictionary<Type, object> services = new Dictionary<Type, object>();

        private ILogger logger;

        internal ServiceLocator() { }

        internal void SetLogger(ILogger logger) => this.logger = logger;

        internal void Bind<T>(T instance) where T:class
        {
            var type = typeof(T);
            if (services.ContainsKey(type))
            {
                logger.Warning($"Attempting to bind {type.Name} multiple times");
                return;
            }
            services.Add(type, instance);
        }

        public bool TryResolve<T>(out T service) where T:class
        {
            if (services.TryGetValue(typeof(T),out var existingService))
            {
                service = (T) existingService;
                return true;
            }
            service = null;
            return false;
        }

    }
}