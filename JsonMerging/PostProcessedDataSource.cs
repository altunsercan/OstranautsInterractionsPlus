using System;
using JetBrains.Annotations;

namespace InteractionsPlus.JsonMerging
{
    internal interface PostProcessedDataSource : JsonDataSource { }
    
    internal class PostProcessedDataSource<TJson> : SimplePostfixOnlyDataSource<TJson>, PostProcessedDataSource
    {
        public readonly string PostProcessMethodName;

        public PostProcessedDataSource([NotNull] ILogger logger, [NotNull] string path, [NotNull] string postProcessMethodName, [NotNull] string dictionaryName, 
        [NotNull] DataHandlerDictionaryAccessor accessor) : base(logger, path, dictionaryName, accessor)
        {
            PostProcessMethodName = postProcessMethodName;
        }
    }
}