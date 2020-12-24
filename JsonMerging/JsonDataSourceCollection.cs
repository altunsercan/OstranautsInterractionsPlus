using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace InteractionsPlus.JsonMerging
{
    internal class JsonDataSourceCollection : IEnumerable<JsonDataSource>
    {
        private const string DataPath = "data/";
        
        [NotNull] private readonly ILogger logger;
        [NotNull] private readonly List<JsonDataSource> jsonPathToDataPath = new List<JsonDataSource>();

        public JsonDataSourceCollection([NotNull] ILogger logger)
        {
            this.logger = logger;
        }

        public void InitializeDataSources()
        {
            var dictionaryFieldList = FindDictionaryFieldList();

            AddDataSource<JsonColor>("colors.json","dictJsonColors");
            AddDataSource<JsonLight>("lights.json","dictLights");
            AddDataSource<JsonGasRespire>("gasrespires.json","dictGasRespires");
            AddDataSource<JsonPowerInfo>("powerinfos.json","dictPowerInfo");
            AddDataSource<JsonGUIPropMap>("guipropmaps.json","dictGUIPropMapUnparsed");
            // DataHandler.ParseGUIPropMaps();
            AddDataSource<JsonCond>("conditions.json","dictConds");
            AddDataSource<JsonSimple>("conditions_simple.json","dictSimple");
            // ParseConditionsSimple();
            AddDataSource<JsonItemDef>("items.json", "dictItemDefs");
            AddDataSource<CondTrigger>("condtrigs.json", "dictCTs");
            AddDataSource<JsonInteraction>("interactions.json", "dictInteractions");
            AddDataSource<JsonCondOwner>("condowners.json", "dictCOs");
            //DataHandler.LoadShips();
            AddDataSource<JsonCondOwner>("loot.json", "dictLoot");
            //DataHandler.TxtToData(DataHandler.strAssetPath + DataHandler.strDataPath + "names_last.json", DataHandler.aNamesLast);
            //DataHandler.dictSimple.Clear();
            AddDataSource<JsonCondOwner>("names_first.json", "dictSimple");
            //DataHandler.ParseFirstNames();
            AddDataSource<JsonHomeworld>("homeworlds.json", "dictHomeworlds");
            AddDataSource<JsonCareer>("careers.json", "dictCareers");
            AddDataSource<JsonLifeEvent>("lifeevents.json", "dictLifeEvents");
            AddDataSource<JsonPersonSpec>("personspecs.json", "dictPersonSpecs");
            //DataHandler.dictSimple.Clear();
            AddDataSource<JsonSimple>("traitscores.json", "dictSimple");
            //DataHandler.ParseTraitScores();
            //DataHandler.dictSimple.Clear();
            AddDataSource<JsonSimple>( "strings.json","dictSimple");
            //DataHandler.ParseGameStrings();
            AddDataSource<JsonPlot>("plots.json", "dictPlots");
            AddDataSource<JsonSlot>("slots.json", "dictSlots");
            AddDataSource<JsonTicker>("tickers.json", "dictTickers");
            AddDataSource<CondRule>("condrules.json", "dictCondRules");
            AddDataSource<JsonAudioEmitter>( "audioemitters.json","dictAudioEmitters");
            //DataHandler.dictSimple.Clear();
            AddDataSource<JsonSimple>("crewskins.json", "dictSimple");
            //DataHandler.ParseCrewSkins();
            AddDataSource<JsonAd>("ads.json", "dictAds");
            AddDataSource<JsonHeadline>("headlines.json", "dictHeadlines");
            AddDataSource<JsonMusic>( "music.json", "dictMusic");
            AddDataSource<JsonComputerEntry>("computerentries.json", "dictComputerEntries");
            //DataHandler.ParseMusic();
            AddDataSource<JsonCOOverlay>("cooverlays.json", "dictCOOverlays");
            //DataHandler.dictSimple.Clear();
            AddDataSource<JsonSimple>("names_ship.json", "dictSimple");
            //DataHandler.ParseShipNames();
            //DataHandler.dictSimple.Clear();
            //DataHandler.ParseGeneratedShipNames();
            AddDataSource<CondTrigger>("condtrigs2.json", "dictCTs");
            AddDataSource<JsonInteraction>("interactions2.json", "dictSocials");
            //DataHandler.dictSocialStats = new Dictionary<string, SocialStats>();
            //foreach (JsonInteraction jsonInteraction in DataHandler.dictSocials.Values)
            //{
            //    DataHandler.dictInteractions[jsonInteraction.strName] = jsonInteraction;
            //    DataHandler.dictSocialStats[jsonInteraction.strName] = new SocialStats(jsonInteraction.strName);
            //}
            //Dictionary<string, JsonInstallable> dict = new Dictionary<string, JsonInstallable>();
            //DataHandler.JsonToData<JsonInstallable>(DataHandler.strAssetPath + DataHandler.strDataPath + "installables.json", dict);
            //foreach (KeyValuePair<string, JsonInstallable> keyValuePair in dict)
            //    Installables.Create(keyValuePair.Value);
            //dict.Clear();
            //if (File.Exists(Application.persistentDataPath + "/settings.json"))
            //    DataHandler.JsonToData<JsonSetting>(Application.persistentDataPath + "/settings.json", DataHandler.dictSettings);
            //DataHandler.bLoaded = true;
            
            
            void Append<T>(string key, object typelessJson, string dictionaryName)
            {
                T typeJson = (T) typelessJson;

                if (!dictionaryFieldList.TryGetValue(dictionaryName, out FieldInfo field))
                {
                    return;
                }
                
                Dictionary<string, T> dictionary = GetDictionary<T>(field);
                if (dictionary.ContainsKey(key))
                {
                    logger.Error($"Cannot append {key}. Key already exists.");
                    return;
                }
                logger.Log($"Data {key} appended");
                
                dictionary.Add(key, typeJson);
            }

            Dictionary<string, T> GetDictionary<T>(FieldInfo field)
            {
                return (Dictionary<string, T>) field.GetValue(null);
            }

            void AddDataSource<T>(string path, string dictionaryName)
            {
                AddSource<T>(DataPath + path, dictionaryName);
            }
            
            void AddSource<T>(string path, string dictionaryName)
            {
                Action<string, object> append = (k, o) => Append<T>(k, o, dictionaryName);
                jsonPathToDataPath.Add(new JsonDataSource(path, typeof(T), append));
            }
        }
        
        private static Dictionary<string, FieldInfo> FindDictionaryFieldList()
        {
            var dictionaryField = new Dictionary<string, FieldInfo>();
            var fieldList = typeof(DataHandler).GetFields();
            foreach (FieldInfo field in fieldList)
            {
                if (!typeof(IDictionary).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                dictionaryField.Add(field.Name, field);
            }

            return dictionaryField;
        }

        public IEnumerator<JsonDataSource> GetEnumerator() => jsonPathToDataPath.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}