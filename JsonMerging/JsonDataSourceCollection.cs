﻿using System;
using System.Collections;
using System.Collections.Generic;
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
            var dictionaryAccessor = new DataHandlerDictionaryAccessor(logger);

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

            AddDataSource<Loot>("loot.json", "dictLoot");
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
            var socialInteractions = new SimplePostfixOnlyDataSource<JsonInteraction>(logger,DataPath + "interactions2.json",
                "dictSocials", dictionaryAccessor);
            socialInteractions.ItemProcessed += AppendSocialInteraction;
            postFixOnlyDataSource.Add(socialInteractions);

            var installables = new SimplePostFixTempDictionarySource<JsonInstallable>(logger, DataPath + "installables.json");
            postFixOnlyDataSource.Add(installables);
            
            AddSource<JsonSetting>("settings.json", "dictSettings");
            
            
            void Append<T>(string key, object typelessJson, string dictionaryName)
            {
                T typeJson = (T) typelessJson;

                Dictionary<string, T> dictionary = dictionaryAccessor.GetDictionary<T>(dictionaryName);
                if (dictionary == null)
                {
                    logger.Error($"Cannot find dictionary target for {dictionaryName}");
                    return;
                }
                
                if (dictionary.ContainsKey(key))
                {
                    logger.Error($"Cannot append {key}. Key already exists.");
                    
                }
                else
                {
                    logger.Log($"Appending data: {key}");
                    dictionary.Add(key, typeJson);   
                }
            }
            
            void AppendSocialInteraction(JsonInteraction jsonInteraction)
            {
                if (jsonInteraction?.strName == null)
                {
                    return;
                }

                var key = jsonInteraction.strName;
                
                Dictionary<string, JsonInteraction> interactionsDict = dictionaryAccessor.GetDictionary<JsonInteraction>("dictInteractions");
                Dictionary<string, SocialStats> socialStatsDict = dictionaryAccessor.GetDictionary<SocialStats>("dictSocialStats");
                if (interactionsDict == null || socialStatsDict == null)
                {
                    return;
                }
                interactionsDict[key] = jsonInteraction;
                socialStatsDict[key] = new SocialStats(key);
            }

            void AppendInstallable(JsonInstallable installable)
            {
                Installables.Create(installable);
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
                    postFixOnlyDataSource.Add(new SimplePostfixOnlyDataSource<T>(logger, path, dictionaryName, dictionaryAccessor));
                }
                else
                {
                    postProcessedDataSources.Add(postParseMethod, new PostProcessedDataSource<T>(logger, path, postParseMethod, dictionaryName, dictionaryAccessor));
                }
            }
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