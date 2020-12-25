using System;

namespace InteractionsPlus.JsonMerging
{
    internal abstract class JsonDataSource
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
            ParseDataSource(modPath);
        }

        protected abstract void ParseDataSource(string modPath);
    }
}