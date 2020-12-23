using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace InteractionsPlus
{
    public class InteractionAdditionalData
    {
        private readonly Interaction interaction;

        private List<Func<bool>> inverseActions;
        private List<Func<bool>> lootActions;
        
        public InteractionAdditionalData(Interaction interaction)
        {
            this.interaction = interaction;
        }

        public void SetAdditionalInverseActions(List<Func<bool>> inverseActions)
        {
            this.inverseActions = inverseActions;
        }

        public void SetAdditionalLootActions(List<Func<bool>> lootActions)
        {
            this.lootActions = lootActions;
        }
    }
}