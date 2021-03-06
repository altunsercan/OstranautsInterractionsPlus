﻿using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
using InteractionsPlus.Exceptions;
using InteractionsPlus.JetBrains.Annotations;
using LitJson;

namespace InteractionsPlus.JsonMerging
{
    internal class JsonParsingUtils
    {
        public delegate void ParseAndAppendDelegate(string modPath, string jsonPath, object untypedDictionary);

        private static ILogger logger;
        [NotNull]
        public static ParseAndAppendDelegate CreateParseDelegate(Type jsonObjectType)
        {
            InteractionsPlusMod.Services.TryResolve(out logger);
            
            Type utilType = typeof(JsonParsingUtils);
            MethodInfo methodInfo = utilType.GetMethod(nameof(ParseAndAppendJsonData));
            
            if (methodInfo == null)
            {
                throw new Exception($"Cannot resolve {nameof(ParseAndAppendJsonData)}");
            }
            
            var typedMethod = methodInfo.MakeGenericMethod(jsonObjectType);

            return (ParseAndAppendDelegate) Delegate.CreateDelegate(typeof(ParseAndAppendDelegate), typedMethod);
        }

        public static void ParseAndAppendJsonData<TJson>([NotNull] string modPath, [NotNull] string jsonPath, object dictionary)
        {
            var typedDictionary = (Dictionary<string, TJson>) dictionary;

            ParseAdditionalJsonInPath<TJson>(modPath, jsonPath,
                (key, parsed) => AppendAdditionalJsonDataToDictionary(typedDictionary, key, parsed, jsonPath));
        }
        
        private static void AppendAdditionalJsonDataToDictionary<TJson>([NotNull]Dictionary<string, TJson> dictionary,
            [NotNull]string key, [NotNull]TJson parsedJson, [NotNull]string jsonPath)
        {
            if (dictionary.ContainsKey(key))
            {
                logger?.Error($"Cannot append {key} to {jsonPath}");
                return;
            }
            logger?.Log($"Data {key} appended to {jsonPath}");
            dictionary.Add(key, parsedJson);
        }

        public static void ParseAdditionalJsonInPath<TJson>([NotNull] string modPath, [NotNull] string jsonPath,
            [NotNull] Action<string, TJson> appendDelegate)
        {
            var additionalJsonPath = Path.Combine(modPath, jsonPath);
            // logger?.Log($"Modpath {modPath} jsonPath{jsonPath} merged {additionalJsonPath}");
            if (!File.Exists(additionalJsonPath))
            {
                return;
            }
            
            string jsonString = File.ReadAllText(additionalJsonPath, Encoding.UTF8);
            JsonData jsonArray = JsonMapper.ToObject(jsonString);
            if (jsonArray == null || !jsonArray.IsArray)
            {
                return;
            }
            
            foreach (JsonData parsedJsonData in jsonArray)
            {
                if (parsedJsonData == null || parsedJsonData["strName"] == null)
                {
                    continue;
                }
                
                var key = parsedJsonData["strName"].ToString();
                appendDelegate(key, JsonMapper.ToObject<TJson>(parsedJsonData.ToJson()));
            }
        }

        public delegate void ParseAndAppendUntypedDelegate(string modPath, string jsonPath, Action<string, object> appendDelegate);
        [NotNull, ItemNotNull]
        private static readonly Dictionary<Type, ParseAndAppendUntypedDelegate> cachedDelegates = new Dictionary<Type, ParseAndAppendUntypedDelegate>();
        
        [NotNull]
        public static ParseAndAppendUntypedDelegate GetParseAdditionalJsonInPathAndAppendTypeless([NotNull] Type jsonType)
        {
            InteractionsPlusMod.Services.TryResolve(out logger);
            
            if (cachedDelegates.TryGetValue(jsonType, out var existing))
            {
                return existing;
            }
            
            var delegateType = typeof(Action<string, string, Action<string, object>>);
            var genericMethod = typeof(JsonParsingUtils).GetMethod(nameof(ParseAdditionalJsonInPathAndAppendTypeless));
            var concreteMethod = genericMethod.MakeGenericMethod(jsonType);

            var @delegate = (Action<string, string, Action<string, object>>) Delegate.CreateDelegate(delegateType, concreteMethod);
            ParseAndAppendUntypedDelegate concreteDelegate = (mp,jp,ap) => @delegate(mp, jp, ap);
            cachedDelegates.Add(jsonType, concreteDelegate);
            return concreteDelegate;
        }
        
        public static void ParseAdditionalJsonInPathAndAppendTypeless<TJson>([NotNull] string modPath, [NotNull] string jsonPath,
            [NotNull] Action<string, object> appendDelegate)
        {
            if (!TryParseAdditionalJsonArrayInPath(modPath, jsonPath, out JsonData jsonArray) || jsonArray == null)
            {
                return;
            }

            foreach (JsonData parsedJsonData in jsonArray)
            {
                if (parsedJsonData == null || parsedJsonData["strName"] == null)
                {
                    logger?.Error($"Empty or no strName");
                    continue;
                }
                ConvertToTypedJsonAndAppend<TJson>(appendDelegate, parsedJsonData);
            }
        }

        public static bool TryParseAdditionalJsonArrayInPath([NotNull] string modPath, [NotNull] string jsonPath, out JsonData jsonArray)
        {
            InteractionsPlusMod.Services.TryResolve(out logger);
            var additionalJsonPath = Path.Combine(modPath, jsonPath);
            if (!File.Exists(additionalJsonPath))
            {
                jsonArray = null;
                return false;
            }

            if (!TryReadAndParseGenericJsonObject(additionalJsonPath, out jsonArray))
            {
                return false;
            }

            if (jsonArray == null || !jsonArray.IsArray)
            {
                logger?.Error($"Not an json array {additionalJsonPath}");
                return false;
            }

            return true;
        }

        private static bool TryReadAndParseGenericJsonObject(string additionalJsonPath, out JsonData jsonArray)
        {
            try
            {
                string jsonString = File.ReadAllText(additionalJsonPath, Encoding.UTF8);
                jsonArray = JsonMapper.ToObject(jsonString);
                return true;
            }
            catch (Exception e)
            {
                logger?.Error($"Cannot parse invalid json format {additionalJsonPath}");
                logger?.LogException(e);
                jsonArray = null;
                return false;
            }
        }

        private static void ConvertToTypedJsonAndAppend<TJson>(Action<string, object> appendDelegate, JsonData parsedJsonData)
        {
            var key = parsedJsonData["strName"].ToString();
            
        }

        public static TJson ConvertToTypedJson<TJson>([NotNull] JsonData parsedJsonData, string debugKey = null)
        {
            try
            {
                return JsonMapper.ToObject<TJson>(parsedJsonData.ToJson());
            }
            catch (Exception e)
            {
                var richException = new JsonFormatException(e, debugKey, typeof(TJson));
                Console.WriteLine(richException);
                throw richException;
            }
        }
    }
}