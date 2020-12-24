using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Text;
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
    }
}