using System.Collections.Generic;
using InteractionsPlus.Handlers;

namespace InteractionsPlus
{
    public class InteractionAdditionalData
    {
        private readonly Interaction interaction;

        private List<LootActionDelegate> lootActions;
        
        public InteractionAdditionalData(Interaction interaction)
        {
            this.interaction = interaction;
        }
        
        public void SetAdditionalLootActions(List<LootActionDelegate> lootActions)
        {
            this.lootActions = lootActions;
        }

        public bool InvokeLootActions(InteractionTriggerArgs args)
        {
            if (lootActions == null)
            {
                return true;
            }
            
            foreach (LootActionDelegate action in lootActions)
            {
                var result = action?.Invoke(args);
                if (result.HasValue && !result.Value)
                {
                    return false;
                }
            }
            return true;
        }
        
    }
}