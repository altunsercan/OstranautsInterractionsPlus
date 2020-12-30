using System.Collections.Generic;
using InteractionsPlus.JetBrains.Annotations;
using LitJson;

namespace InteractionsPlus.JsonMerging
{
    internal abstract class TempDictionarySource<TJson> : JsonDataSource<TJson> 
    {
        public TempDictionarySource([NotNull] ILogger logger, [NotNull] string path) 
            : base(logger, path)
        {
        }
        
        protected override void ParseDataSource(string modPath)
        {
            var jsonPath = Path;

            if (!JsonParsingUtils.TryParseAdditionalJsonArrayInPath(modPath, jsonPath, out JsonData jsonArray))
            {
                return;
            }
            
            Dictionary<string, TJson> dictionary = new Dictionary<string, TJson>();
            Appender.ProcessJsonArray(dictionary, jsonArray);
            dictionary.Clear();
        }
    }
}