using InteractionsPlus.JetBrains.Annotations;

namespace InteractionsPlus.JsonMerging
{
    internal interface SimplePostfixOnlyDataSource : JsonDataSource { }
    
    internal class SimplePostfixOnlyDataSource<TJson> : DataHandlerDictionarySource<TJson>, SimplePostfixOnlyDataSource
    {
        public SimplePostfixOnlyDataSource([NotNull] ILogger logger, [NotNull] string path, [NotNull] string dictionaryName, 
            [NotNull] DataHandlerDictionaryAccessor accessor) : base(logger, path, dictionaryName, accessor)
        {
        }
    }
}