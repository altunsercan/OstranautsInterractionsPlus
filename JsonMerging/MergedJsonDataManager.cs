using System;
using System.IO;
using System.Text;
using JetBrains.Annotations;
using LitJson;

namespace InteractionsPlus.JsonMerging
{
    internal class MergedJsonDataManager
    {
        private bool initialized;
     
        [NotNull] private readonly ILogger logger;
        [NotNull] private readonly JsonDataSourceCollection jsonDataSourceCollection;

        public MergedJsonDataManager([NotNull] ILogger logger)
        {
            this.logger = logger;
            jsonDataSourceCollection = new JsonDataSourceCollection(logger);
        }
        
        public void Initialized()
        {
            if (initialized)
            {
                return;
            }

            initialized = true;
            jsonDataSourceCollection.InitializeDataSources();
        }
        
        public void LoadJsonData(string modPath)
        {
            foreach (JsonDataSource dataSource in jsonDataSourceCollection)
            {
                ParseDataSource(modPath, dataSource);
            }
        }
        
        private void ParseDataSource(string modPath, JsonDataSource dataSource)
        {
            var jsonPath = dataSource.Path;
            
            logger.Log($"Parsing additional json {modPath} {jsonPath} {dataSource.AppendAction!=null}");

            var appendMethod = typeof(MergedJsonDataManager).GetMethod(nameof(ParseAdditionalJsonInPath));
            appendMethod.MakeGenericMethod(dataSource.JsonType)
                .Invoke(null, new object[] {modPath,  dataSource.Path, dataSource.AppendAction});
        }
        
        public static void ParseAdditionalJsonInPath<TJson>([NotNull] string modPath, [NotNull] string jsonPath,
            [NotNull] Action<string, object> appendDelegate)
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