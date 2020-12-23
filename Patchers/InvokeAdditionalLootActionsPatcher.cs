using System;
using HarmonyLib;
using JetBrains.Annotations;

namespace InteractionsPlus.Patchers
{
    /*  public bool Triggered(
    CondOwner objUs,
    CondOwner objThem,
    bool bStats = false,
    bool bIgnoreItems = false,
    bool bCheckPath = false)
    */
    
    [HarmonyPatch(typeof(Interaction), nameof(Interaction.Triggered), MethodType.Normal)]
    [HarmonyPatch(new Type[]
    {typeof(CondOwner), typeof(CondOwner), typeof(bool), typeof(bool), typeof(bool)})]
    public class InvokeAdditionalLootActionsPatcher
    {
        static void Postfix([NotNull] Interaction __instance, ref bool __result, 
            CondOwner __0, CondOwner __1, bool __2, bool __3, bool __4)
        {
            InteractionsPlusMod.Services.TryResolve(out ILogger logger);
            
            if (!__result)
            {
                return;
            }

            InteractionAdditionalData additionalData = null;
            InteractionAdditionalActionsManager additionalActionsManager = null;
            if(!InteractionsPlusMod.Services.TryResolve(out additionalActionsManager) 
               || additionalActionsManager == null
               || !additionalActionsManager.TryGetAdditionalData(__instance, out additionalData)
               || additionalData == null)
            {
                logger?.Error($"InvokeLootAction has missing data {__instance.strName}");
                return;
            }

            __result = additionalData.InvokeLootActions();
        }
    }
    
    
}