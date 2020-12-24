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
    }
}