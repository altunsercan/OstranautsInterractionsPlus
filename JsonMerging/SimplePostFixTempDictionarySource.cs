using InteractionsPlus.JetBrains.Annotations;

namespace InteractionsPlus.JsonMerging
{
    internal class SimplePostFixTempDictionarySource<TJson> : TempDictionarySource<TJson>, SimplePostfixOnlyDataSource
    {
        public SimplePostFixTempDictionarySource([NotNull] ILogger logger, [NotNull] string path) : base(logger, path)
        {
        }
    }
}