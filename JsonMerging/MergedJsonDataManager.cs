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
                dataSource.ParseModPath(modPath);
            }
        }
        
        
    }
}