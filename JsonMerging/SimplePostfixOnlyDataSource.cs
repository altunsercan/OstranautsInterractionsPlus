using System;

namespace InteractionsPlus.JsonMerging
{
    internal class SimplePostfixOnlyDataSource : JsonDataSource
    {
        public SimplePostfixOnlyDataSource(string path, Type type, Action<string, object> appendAction)
            : base(path, type, appendAction)
        {
        }

        protected override void ParseDataSource(string modPath)
        {
            var jsonPath = Path;
            var parseDelegate = JsonParsingUtils.GetParseAdditionalJsonInPathAndAppendTypeless(JsonType);
            parseDelegate(modPath,  jsonPath, AppendAction);
        }
    }
}