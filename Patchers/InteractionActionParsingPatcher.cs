using System;
using System.Collections.Generic;
using HarmonyLib;
using InteractionsPlus.Handlers;
using JetBrains.Annotations;

namespace InteractionsPlus.Patchers
{
    [HarmonyPatch(typeof(Interaction))]
    [HarmonyPatch(MethodType.Constructor)]
    [HarmonyPatch(new Type[]{typeof(JsonInteraction), typeof(JsonInteractionSave)})]
    public class InteractionActionParsingPatcher
    {
        private static ILogger logger;
        static void Postfix([NotNull]Interaction __instance, JsonInteraction __0)
        {
            InteractionsPlusMod.Services.TryResolve(out logger);
            
            Interaction interaction = __instance;
            if (!InteractionsPlusMod.Services.TryResolve(out HandlerManager handlerManager) ||
                !InteractionsPlusMod.Services.TryResolve(out InteractionAdditionalActionsManager additionalsManager)
            )
            {
                return;
            }

            var additionalData = additionalsManager.CreateAdditionalData(interaction);
            if (__0.aInverse != null)
            {
                List<Func<bool>> inverseAction = ParseActions(handlerManager, __0.aInverse);
                additionalData.SetAdditionalInverseActions(inverseAction);
            }

            if (__0.aLootItms != null)
            {
                List<Func<bool>> lootActions = ParseActions(handlerManager, __0.aLootItms);
                additionalData.SetAdditionalLootActions(lootActions);
            }
            
            
            return;
        }

        private static List<Func<bool>> ParseActions(HandlerManager manager, [NotNull] string[] actionStrArr)
        {
            List<Func<bool>> actions = new List<Func<bool>>();
            foreach (string actionItemStr in actionStrArr)
            {
                AppendAction(actions, ParseAction(manager, actionItemStr));
            }
            return actions;
        }

        private static void AppendAction([NotNull]List<Func<bool>> actions, Func<bool> parseAction)
        {
            if (parseAction == null)
            {
                return;
            }
            actions.Add(parseAction);
        }
        
        private static Func<bool> ParseAction(HandlerManager manager, string actionItemStr)
        {
            string[] tokens = actionItemStr.Split(',');
            if (tokens.Length == 0)
            {
                return null;
            }
            
            var name = tokens[0];
            if (!manager.TryGetHandlerViaName(name, out HandlerMethodCache cache))
            {
                return null;
            }

            if (tokens.Length-1 != cache.ParameterTypes.Length)
            {
                return null;
            }

            object[] parameters = new object[tokens.Length-1];
            for (var index = 0; index < cache.ParameterTypes.Length; index++)
            {
                Type parameterType = cache.ParameterTypes[index];
                parameters[index] = ConvertParameter(parameterType, tokens[index + 1]);
            }

            Func<bool> callDelegate = ()=>
            {
                logger?.Log($"Action called {actionItemStr}");
                
                object result = cache.Delegate.DynamicInvoke(parameters);
                if (result is bool boolResult)
                {
                    return boolResult;
                }
                return true;
            };
            return callDelegate;
        }

        private static object ConvertParameter(Type parameterType, string token)
        {
            if (parameterType == typeof(string))
            {
                return token;
            }else if (parameterType == typeof(int))
            {
                return int.Parse(token);
            }else if (parameterType == typeof(bool))
            {
                return bool.Parse(token);
            }else if (parameterType == typeof(float))
            {
                return float.Parse(token);
            }else if (parameterType == typeof(double))
            {
                return double.Parse(token);
            }
            // TODO: Add custom type parsers
            return null;
        }
    }
}