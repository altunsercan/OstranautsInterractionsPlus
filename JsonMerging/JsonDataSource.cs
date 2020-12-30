using System;
using JetBrains.Annotations;

namespace InteractionsPlus.JsonMerging
{
    internal interface JsonDataSource
    {
        void ParseModPath(string modPath);
    }
    
    internal abstract class JsonDataSource<TJson> : JsonDataSource
    {
        [NotNull] public readonly string Path;
        [NotNull] public readonly Type JsonType;
        [NotNull] protected readonly JsonAppender<TJson> Appender;

        public event Action<TJson> ItemProcessed; 
        public event Action<TJson> ItemAppended;
        public event Action<TJson> ItemModified;

        public JsonDataSource([NotNull] ILogger logger,  [NotNull] string path)
        {
            Path = path;
            JsonType = typeof(TJson);
            
            Appender = new JsonAppender<TJson>(logger);
            Appender.ItemParsed += OnItemProcess;
            ItemAppended += ItemProcessed;
            ItemModified += ItemProcessed;
        }

        public void ParseModPath([NotNull]string modPath)
        {
            ParseDataSource(modPath);
        }
        protected abstract void ParseDataSource([NotNull]string modPath);
        
        protected virtual void OnItemProcess(JsonAppender<TJson>.ItemProcessedArgs itemProcessedArgs)
        {
            if (itemProcessedArgs.IsNewObject)
            {
                DispatchAppendedEvent(itemProcessedArgs.ParsedObject);
            }
            else
            {
                DispatchModifiedEvent(itemProcessedArgs.ParsedObject);
            }
        }

        protected void DispatchAppendedEvent(TJson parsedObject) => ItemAppended?.Invoke(parsedObject);
        protected void DispatchModifiedEvent(TJson parsedObject) => ItemModified?.Invoke(parsedObject);
    }
}