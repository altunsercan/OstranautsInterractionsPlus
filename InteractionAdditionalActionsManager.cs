using System.Collections.Generic;
using InteractionsPlus.JetBrains.Annotations;

namespace InteractionsPlus
{
    public class InteractionAdditionalActionsManager
    {
        [NotNull]
        private readonly Dictionary<Interaction, InteractionAdditionalData> additionalDatas 
            = new Dictionary<Interaction, InteractionAdditionalData>();
        
        public InteractionAdditionalData CreateAdditionalData(Interaction interaction)
        {
            var additional = new InteractionAdditionalData(interaction);
            additionalDatas.Add(interaction, additional);
            return additional;
        }

        public bool TryGetAdditionalData(Interaction interaction, out InteractionAdditionalData additional) =>
            additionalDatas.TryGetValue(interaction, out additional);
    }
}