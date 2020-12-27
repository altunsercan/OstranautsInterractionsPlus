using System;

namespace InteractionsPlus.JsonMerging
{
    internal class PostProcessedDataSource : SimplePostfixOnlyDataSource
    {
        public readonly string PostProcessMethodName;

        public PostProcessedDataSource(string path, Type type, Action<string, object> appendAction, string postProcessMethodName) : base(path, type, appendAction)
        {
            PostProcessMethodName = postProcessMethodName;
        }
    }
}