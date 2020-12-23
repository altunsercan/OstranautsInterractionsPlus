using System;
using System.Collections.Generic;
using System.Linq;
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
            if (__0.aLootItms != null)
            {
                List<LootActionDelegate> lootActions = ParseActions(handlerManager, __instance.strName, __0.aLootItms);
                additionalData.SetAdditionalLootActions(lootActions);
            }
            return;
        }

        private static List<LootActionDelegate> ParseActions(HandlerManager manager, string interactionName,
            [NotNull] string[] actionStrArr)
        {
            List<LootActionDelegate> actions = new List<LootActionDelegate>();
            foreach (string actionItemStr in actionStrArr)
            {
                AppendAction(actions, ParseAction(manager, interactionName, actionItemStr));
            }
            return actions;
        }

        private static void AppendAction([NotNull]List<LootActionDelegate> actions, LootActionDelegate parseAction)
        {
            if (parseAction == null)
            {
                return;
            }
            actions.Add(parseAction);
        }
        
        private static LootActionDelegate ParseAction(
            [NotNull] HandlerManager manager, 
            [NotNull] string interactionName,
            [NotNull] string actionItemStr)
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

            var specialInjectType = typeof(InteractionTriggerArgs);

            var hasSpecialInject = cache.ParameterTypes.Contains(specialInjectType);
            var tokensNeeded = cache.ParameterTypes.Length + 1 - ((hasSpecialInject) ? 1 : 0);
            
            if (tokens.Length != tokensNeeded)
            {
                logger?.Error($"Incorrect number of tokens for {name} in {interactionName}. Expected {tokensNeeded}, got {tokens.Length-1}");
                return null;
            }

            int triggerArgIndex = -1;
            int tokenIndexShift = 1;
            object[] parameters = new object[cache.ParameterTypes.Length];
            for (var index = 0; index < cache.ParameterTypes.Length; index++)
            {
                Type parameterType = cache.ParameterTypes[index];
                if (parameterType == specialInjectType)
                {
                    triggerArgIndex = index;
                    tokenIndexShift--;
                    continue;
                }
                
                parameters[index] = ConvertParameter(parameterType, tokens[index + tokenIndexShift]);
            }

            LootActionDelegate callDelegate = (args)=>
            {
                if (triggerArgIndex != -1)
                {
                    parameters[triggerArgIndex] = args;
                }
                return cache.Delegate(parameters);
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