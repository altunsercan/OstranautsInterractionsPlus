using System.Collections.Generic;
using HarmonyLib;
using InteractionsPlus.JsonMerging;
using InteractionsPlus.ModDependency;

namespace InteractionsPlus.Patchers
{
    [HarmonyPatch(typeof(DataHandler))]
    public class DataHandlerInitPostfixPatcher
    {
        private static ILogger logger;
        
        [HarmonyPostfix]
        [HarmonyPatch(nameof(DataHandler.Init))]
        static void ConstructorPostfix()
        {
            InteractionsPlusMod.Services.TryResolve(out logger);
            
            if (!InteractionsPlusMod.Services.TryResolve(out UMMDependencyManager dependencyManager)||dependencyManager==null ||
                !InteractionsPlusMod.Services.TryResolve(out MergedJsonDataManager jsonDataCache)|| jsonDataCache==null )
            {
                return;
            }
            
            jsonDataCache.Initialized();
            
            List<string> modPathList = dependencyManager.GetDependentModPaths(true);
            foreach (string modPath in modPathList)
            {
                jsonDataCache.LoadPostFixJsonData(modPath);
            }
        }
    }
}