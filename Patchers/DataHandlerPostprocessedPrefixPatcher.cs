using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using HarmonyLib;
using InteractionsPlus.JsonMerging;
using InteractionsPlus.ModDependency;
using UnityEngine;

namespace InteractionsPlus.Patchers
{
    [HarmonyPatch]
    public class DataHandlerPostprocessedPrefixPatcher
    {
        static IEnumerable<MethodBase> TargetMethods()
        {
            InteractionsPlusMod.Services.TryResolve(out logger);
            var handlerType = typeof(DataHandler);

            HashSet<string> foundMethods = new HashSet<string>();
            string[] targetMethodNames = new []
                {
                    "ParseGUIPropMaps",
                    "ParseConditionsSimple",
                    "ParseFirstNames",
                    "ParseTraitScores",
                    "ParseGameStrings",
                    "ParseCrewSkins",
                    "ParseMusic",
                    "ParseShipNames"
                };

            foreach (MethodInfo methodBase in handlerType.GetMethods(BindingFlags.NonPublic|BindingFlags.Static))
            {
                string methodName = methodBase.Name;
                if (!targetMethodNames.Contains(methodName))
                {
                    continue;
                }
                foundMethods.Add(methodName);
                yield return methodBase;
            }

            foreach (string targetMethodName in targetMethodNames)
            {
                if (!foundMethods.Contains(targetMethodName))
                {
                    logger?.Error($"Could not find {targetMethodName} to patch");
                }
            }
        }
        
        private static ILogger logger;
        
        static void Prefix(MethodBase __originalMethod)
        {
            InteractionsPlusMod.Services.TryResolve(out logger);
            
            if (!InteractionsPlusMod.Services.TryResolve(out UMMDependencyManager dependencyManager)||dependencyManager==null ||
                !InteractionsPlusMod.Services.TryResolve(out MergedJsonDataManager jsonDataCache)|| jsonDataCache==null )
            {
                return;
            }

            Quaternion.LookRotation(Vector3.forward, Vector3.up);
            
            jsonDataCache.Initialized();
            string methodName = __originalMethod.Name;
            
            List<string> modPathList = dependencyManager.GetDependentModPaths(true);
            foreach (string modPath in modPathList)
            {
                jsonDataCache.LoadPostProcessing(modPath, methodName);
            }
        }
    }
}