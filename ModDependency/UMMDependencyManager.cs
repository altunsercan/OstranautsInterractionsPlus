using System.Collections.Generic;
using JetBrains.Annotations;
using UnityModManagerNet;

namespace InteractionsPlus.ModDependency
{
    public class UMMDependencyManager
    {
        [NotNull] private readonly ILogger logger;

        private List<UnityModManager.ModEntry> modEntries;
        
        internal UMMDependencyManager([NotNull] ILogger logger)
        {
            this.logger = logger;
        }
        
        internal void SetModList(List<UnityModManager.ModEntry> modEntries)
        {
            this.modEntries = modEntries;
        }

        [NotNull]
        public List<string> GetDependentModPaths(bool enabledOnly = true)
        {
            var pathList = new List<string>();
            if (modEntries == null)
            {
                return pathList;
            }
            
            foreach (var entry in modEntries)
            {
                if (
                    !RequiresInteractionsPlus(entry)
                    || (enabledOnly && !entry.Enabled)
                )
                {
                    continue;
                }
                pathList.Add(entry.Path);
            }
            return pathList;
        }

        private bool RequiresInteractionsPlus(UnityModManager.ModEntry entry)
        {
            return entry.Requirements.ContainsKey(InteractionsPlusMod.ModId);
        }
    }
}