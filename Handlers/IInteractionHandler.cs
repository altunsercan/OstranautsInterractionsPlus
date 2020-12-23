using System;

namespace InteractionsPlus.Handlers
{
    [AttributeUsage(AttributeTargets.Method)]
    public class InteractionHandlerAttribute : Attribute { }

    public delegate bool LootActionDelegate(InteractionTriggerArgs triggerArgs);
}