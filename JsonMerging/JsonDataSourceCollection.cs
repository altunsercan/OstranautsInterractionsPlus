using System;
using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace InteractionsPlus.JsonMerging
{
    internal class JsonDataSourceCollection
    {
        private const string DataPath = "data\\";
        
        [NotNull] private readonly ILogger logger;
        [NotNull] private readonly List<SimplePostfixOnlyDataSource> postFixOnlyDataSource = new List<SimplePostfixOnlyDataSource>();
        [NotNull] private readonly Dictionary<string, PostProcessedDataSource> postProcessedDataSources = new Dictionary<string, PostProcessedDataSource>();

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
            AddDataSource<JsonGUIPropMap>("guipropmaps.json","dictGUIPropMapUnparsed", postParseMethod: "ParseGUIPropMaps");
            AddDataSource<JsonCond>("conditions.json","dictConds");
            AddSimpleSource("conditions_simple.json","ParseConditionsSimple");
            AddDataSource<JsonItemDef>("items.json", "dictItemDefs");
            AddDataSource<CondTrigger>("condtrigs.json", "dictCTs");
            AddDataSource<JsonInteraction>("interactions.json", "dictInteractions");
            AddDataSource<JsonCondOwner>("condowners.json", "dictCOs");
            
            //DataHandler.LoadShips(); // Multi json source

            AddDataSource<JsonCondOwner>("loot.json", "dictLoot");
            //DataHandler.TxtToData(DataHandler.strAssetPath + DataHandler.strDataPath + "names_last.json", DataHandler.aNamesLast);
            //DataHandler.dictSimple.Clear();
            AddSimpleSource("names_first.json", "ParseFirstNames");
            AddDataSource<JsonHomeworld>("homeworlds.json", "dictHomeworlds");
            AddDataSource<JsonCareer>("careers.json", "dictCareers");
            AddDataSource<JsonLifeEvent>("lifeevents.json", "dictLifeEvents");
            AddDataSource<JsonPersonSpec>("personspecs.json", "dictPersonSpecs");
            //DataHandler.dictSimple.Clear();
            AddSimpleSource("traitscores.json", "ParseTraitScores");
            //DataHandler.dictSimple.Clear();
            AddSimpleSource("strings.json","ParseGameStrings");
            AddDataSource<JsonPlot>("plots.json", "dictPlots");
            AddDataSource<JsonSlot>("slots.json", "dictSlots");
            AddDataSource<JsonTicker>("tickers.json", "dictTickers");
            AddDataSource<CondRule>("condrules.json", "dictCondRules");
            AddDataSource<JsonAudioEmitter>( "audioemitters.json","dictAudioEmitters");
            //DataHandler.dictSimple.Clear();
            AddSimpleSource("crewskins.json", "ParseCrewSkins");
            AddDataSource<JsonAd>("ads.json", "dictAds");
            AddDataSource<JsonHeadline>("headlines.json", "dictHeadlines");
            AddDataSource<JsonComputerEntry>("computerentries.json", "dictComputerEntries");
            AddDataSource<JsonMusic>("music.json", "dictMusic", "ParseMusic");
            AddDataSource<JsonCOOverlay>("cooverlays.json", "dictCOOverlays");
            //DataHandler.dictSimple.Clear();
            AddSimpleSource("names_ship.json", "ParseShipNames");
            //DataHandler.dictSimple.Clear();
            
            //DataHandler.ParseGeneratedShipNames();  // Multi json
            
            AddDataSource<CondTrigger>("condtrigs2.json", "dictCTs");
            
            // Special handling of interaction2.json
            postFixOnlyDataSource.Add(new SimplePostfixOnlyDataSource(DataPath+"interactions2.json", typeof(JsonInteraction), AppendSocialInteraction));
            postFixOnlyDataSource.Add(new SimplePostfixOnlyDataSource(DataPath+"installables.json", typeof(JsonInstallable), AppendInstallable));
            
            AddSource<JsonSetting>("settings.json", "dictSettings");
            
            
            void Append<T>(string key, object typelessJson, string dictionaryName)
            {
                T typeJson = (T) typelessJson;

                Dictionary<string, T> dictionary = GetDictionary<T>(dictionaryName);
                if (dictionary == null)
                {
                    return;
                }
                
                if (dictionary.ContainsKey(key))
                {
                    logger.Error($"Cannot append {key}. Key already exists.");
                    return;
                }
                logger.Log($"Data {key} appended");
                
                dictionary.Add(key, typeJson);
            }
            
            void AppendSocialInteraction(string key, object typelessJson)
            {
                if (typelessJson == null)
                {
                    return;
                }
                
                Append<JsonInteraction>(key, typelessJson, "dictSocials");

                JsonInteraction jsonInteraction =  (JsonInteraction) typelessJson;
                
                Dictionary<string, JsonInteraction> interactionsDict = GetDictionary<JsonInteraction>("dictInteractions");
                Dictionary<string, SocialStats> socialStatsDict = GetDictionary<SocialStats>("dictSocialStats");
                if (interactionsDict == null || socialStatsDict == null)
                {
                    return;
                }
                interactionsDict[jsonInteraction.strName] = jsonInteraction;
                socialStatsDict[jsonInteraction.strName] = new SocialStats(jsonInteraction.strName);
            }

            void AppendInstallable(string key, object typelessJson)
            {
                JsonInstallable installable = (JsonInstallable) typelessJson;
                
                Installables.Create(installable);
            }
            

            Dictionary<string, T> GetDictionary<T>(string dictionaryName)
            {
                if (!dictionaryFieldList.TryGetValue(dictionaryName, out FieldInfo field))
                {
                    logger.Error($"Cannot find data dictionary {dictionaryName}.");
                    return null;
                }
                
                return GetDictionaryFromFieldInfo<T>(field);
            }
            
            Dictionary<string, T> GetDictionaryFromFieldInfo<T>(FieldInfo field)
            {
                return (Dictionary<string, T>) field.GetValue(null);
            }

            void AddSimpleSource(string path, string postParseMethod)
            {
                AddSource<JsonSimple>(DataPath + path, "dictSimple", postParseMethod);
            }

            void AddDataSource<T>(string path, string dictionaryName, string postParseMethod = null)
            {
                AddSource<T>(DataPath + path, dictionaryName, postParseMethod);
            }
            
            void AddSource<T>(string path, string dictionaryName, string postParseMethod = null)
            {
                Action<string, object> append = (k, o) => Append<T>(k, o, dictionaryName);

                if (postParseMethod==null)
                {
                    postFixOnlyDataSource.Add(new SimplePostfixOnlyDataSource(path, typeof(T), append));
                }
                else
                {
                    postProcessedDataSources.Add(postParseMethod, new PostProcessedDataSource(path, typeof(T), append, postParseMethod));
                }
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

        public IEnumerator EnumeratePostFixDataSources() => postFixOnlyDataSource.GetEnumerator();

        public PostProcessedDataSource GetPostProcessedSourceFor(string postProcessMethodName)
        {
            if (postProcessedDataSources.TryGetValue(postProcessMethodName, out var source))
            {
                return source;
            }
            return null;
        }
    }
}