using System;
using System.Collections.Generic;
using JetBrains.Annotations;
using LitJson;
using TinyJson;

namespace InteractionsPlus.JsonMerging
{
    internal class JsonAppender<TJson>
    {
        public delegate void ItemParsedHandler(ItemProcessedArgs itemProcessedArgs);

        public event ItemParsedHandler ItemParsed;
        
        [NotNull]
        private readonly ILogger logger;

        public JsonAppender([NotNull] ILogger logger)
        {
            this.logger = logger;
        }
        
        public void ProcessJsonArray([NotNull] Dictionary<string,TJson> targetDictionary, [NotNull] JsonData jsonArray)
        {
            int emptyItemFound = 0;
            foreach (JsonData objectJsonData in jsonArray)
            {
                ProcessItem(targetDictionary, objectJsonData, ref emptyItemFound);
            }

            if (emptyItemFound != 0)
            {
                logger.Error($"{emptyItemFound} items found empty or with no strName assigned");
            }
        }

        private void ProcessItem([NotNull] Dictionary<string, TJson> targetDictionary, JsonData objectJsonData, ref int emptyItemFound)
        {
            if (objectJsonData?["strName"] == null)
            {
                emptyItemFound++;
                return;
            }

            var key = objectJsonData["strName"].ToString();

            if (targetDictionary.ContainsKey(key))
            {
                ApplyItemOverrides(targetDictionary, objectJsonData, key);
            }
            else
            {
                AppendItem(targetDictionary, objectJsonData, key);
            }
        }

        private void ApplyItemOverrides([NotNull] Dictionary<string, TJson> targetDictionary,
            [NotNull] JsonData objectJsonData, [NotNull] string key)
        {
            logger.Log($"Checking for override {key}");

            Dictionary<string, string> overrideStrategies = MakeOverrideKeyToStrategyMap(objectJsonData);
            if (overrideStrategies.Count == 0)
            {
                logger.Log("Cannot find valid override. Skipping.");
                return;
            }

            TJson original = targetDictionary[key];
            JsonData originalAsData = JsonMapper.ToObject(original.ToJson());
            
            foreach (KeyValuePair<string, string> kvp in overrideStrategies)
            {
                var overrideKey = kvp.Key;
                var strategy = kvp.Value;
                
                if (overrideKey == null || strategy == null)
                {
                    continue;
                }
                
                ApplyOverrideStrategy(originalAsData, objectJsonData, overrideKey, strategy);
            }
            
            if (TryConvertToTypedJson(originalAsData, key, out TJson overriden))
            {
                targetDictionary[key] = overriden;
            }
        }

        private void ApplyOverrideStrategy([NotNull] JsonData originalAsData, [NotNull] JsonData overrideData,
            [NotNull] string overrideKey, [NotNull] string strategy)
        {
            var jsonToSource = overrideData[overrideKey];
            var jsonToTarget = originalAsData[overrideKey];
            if (jsonToTarget == null || jsonToSource == null)
            {
                return;
            }

            if (strategy == "append")
            {
                ApplyAppendOverride(jsonToSource, jsonToTarget, overrideKey);
            }
            else if (strategy == "overwrite")
            {
                ApplyOverwriteOverride(jsonToSource, jsonToTarget, overrideKey);
            }
            else if (strategy == "remove")
            {
                ApplyRemoveOverride(jsonToSource, jsonToTarget, overrideKey);
            }
        }
        
        private void ApplyAppendOverride([NotNull] JsonData jsonToSource, [NotNull] JsonData jsonToTarget, string debugKey)
        {
            if (!jsonToSource.IsArray || !jsonToTarget.IsArray)
            {
                logger.Log($"Cannot append {debugKey}. Not a valid array json");
                return;
            }

            int count = 0;
            foreach (JsonData data in jsonToSource)
            {
                jsonToTarget.Add(data);
                count++;
            }
            logger.Log($"Appended {count} items to {debugKey}.");
        }
        
        private void ApplyOverwriteOverride([NotNull] JsonData jsonToSource, [NotNull] JsonData jsonToTarget, string debugKey)
        {
            if (!jsonToSource.IsArray || !jsonToTarget.IsArray)
            {
                logger.Log($"Cannot append {debugKey}. Not a valid array json");
                return;
            }
         
            jsonToTarget.Clear();
            int count = 0;
            foreach (JsonData data in jsonToSource)
            {
                jsonToTarget.Add(data);
                count++;
            }
            logger.Log($"Overwritten with {count} items to {debugKey}.");
        }
        
        private void ApplyRemoveOverride([NotNull] JsonData jsonToSource, [NotNull] JsonData jsonToTarget, string debugKey)
        {
            if (!jsonToSource.IsArray || !jsonToTarget.IsArray)
            {
                logger.Log($"Cannot append {debugKey}. Not a valid array json");
                return;
            }
            
            HashSet<string> dataToKeep = new HashSet<string>();
            foreach (JsonData existingData in jsonToTarget)
            {
                dataToKeep.Add(existingData.ToJson());
            }

            int count = 0;
            foreach (JsonData toRemoveData in jsonToSource)
            {
                dataToKeep.Remove(toRemoveData.ToJson());
                count++;
            }

            jsonToTarget.Clear();

            foreach (string jsonStr in dataToKeep)
            {
                jsonToTarget.Add(JsonMapper.ToObject(jsonStr));
            }
            logger.Log($"Removed {count} items from {debugKey}.");
        }

        [NotNull]
        private Dictionary<string,string> MakeOverrideKeyToStrategyMap([NotNull] JsonData jsonData)
        {
            const string overrideKeyword = ".override";
            var dictionary = new Dictionary<string, string>();
            var keys = jsonData.Keys;
            if (keys == null)
            {
                logger.Error("JsonData is missing keys");
                return dictionary;
            }
            
            foreach (string propertyKey in keys)
            {
                AddOverrideStrategyForProperty(propertyKey);
            }
            return dictionary;

            void AddOverrideStrategyForProperty(string propertyKey)
            {
                if (propertyKey == null || !propertyKey.EndsWith(overrideKeyword))
                {
                    return;
                }

                JsonData strategyData = jsonData[propertyKey];
                string overrideStrategy = (strategyData != null) ? strategyData.ToString() : "overwrite";
                string overriddenKey = propertyKey.Substring(0, propertyKey.Length - overrideKeyword.Length);
                dictionary.Add(overriddenKey, overrideStrategy);
            }
        }

        private void AppendItem([NotNull] Dictionary<string, TJson> targetDictionary, [NotNull] JsonData parsedJsonData, [NotNull] string key)
        {
            logger.Log($"Appending {key}");

            if (!TryConvertToTypedJson(parsedJsonData, key, out TJson json) || json == null)
            {
                return;
            }
            
            targetDictionary.Add(key, json);
            ItemParsed?.Invoke(new ItemProcessedArgs(key, json, isNewObject: true));
        }

        private bool TryConvertToTypedJson([NotNull] JsonData parsedJsonData, [NotNull] string key, out TJson json)
        {
            try
            {
                json = JsonParsingUtils.ConvertToTypedJson<TJson>(parsedJsonData, key);
                return true;
            }
            catch (Exception e)
            {
                logger.LogException(e);
                json = default(TJson);
                return false;
            }
        }

        public struct ItemProcessedArgs
        {
            [NotNull] public readonly string Key;
            [NotNull] public readonly TJson ParsedObject;
            public readonly bool IsNewObject;
            public ItemProcessedArgs([NotNull] string key, [NotNull] TJson parsedObject, bool isNewObject)
            {
                Key = key;
                ParsedObject = parsedObject;
                IsNewObject = isNewObject;
            }
        }
    }
}