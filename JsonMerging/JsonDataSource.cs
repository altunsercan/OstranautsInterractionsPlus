using System;

namespace InteractionsPlus.JsonMerging
{
    internal class JsonDataSource
    {
        public readonly string Path;
        public readonly Type JsonType;
        public readonly Action<string, object> AppendAction;
            
        public JsonDataSource(string path, Type type, Action<string,object> appendAction)
        {
            Path = path;
            JsonType = type;
            AppendAction = appendAction;
        }

        public void ParseModPath(string modPath)
        {
            ParseDataSource(modPath, this);
        }
        
        private void ParseDataSource(string modPath, JsonDataSource dataSource)
        {
            var jsonPath = dataSource.Path;
            var parseDelegate = JsonParsingUtils.GetParseAdditionalJsonInPathAndAppendTypeless(dataSource.JsonType);
            parseDelegate(modPath,  jsonPath, dataSource.AppendAction);
        }
        
    }
}