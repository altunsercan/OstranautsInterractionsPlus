using System;
using System.Collections.Generic;
using InteractionsPlus.JetBrains.Annotations;
using LitJson;

namespace InteractionsPlus.JsonMerging
{
    internal abstract class DataHandlerDictionarySource<TJson> : JsonDataSource<TJson> 
    {
        [NotNull] protected readonly string DictionaryName;
        [NotNull] protected readonly DataHandlerDictionaryAccessor Accessor;

        public DataHandlerDictionarySource([NotNull] ILogger logger, [NotNull] string path,
            [NotNull] String dictionaryName, [NotNull] DataHandlerDictionaryAccessor accessor) 
            : base(logger, path)
        {
            DictionaryName = dictionaryName;
            Accessor = accessor;
        }
        
        protected override void ParseDataSource(string modPath)
        {
            var jsonPath = Path;

            if (!JsonParsingUtils.TryParseAdditionalJsonArrayInPath(modPath, jsonPath, out JsonData jsonArray))
            {
                return;
            }
            
            // Access dictionary just in time to avoid fetching null or old dictionary.
            Dictionary<string, TJson> dictionary = Accessor.GetDictionary<TJson>(DictionaryName);
            Appender.ProcessJsonArray(dictionary, jsonArray);
        }
    }
}