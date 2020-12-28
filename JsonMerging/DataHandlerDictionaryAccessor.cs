using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using JetBrains.Annotations;

namespace InteractionsPlus.JsonMerging
{
    internal class DataHandlerDictionaryAccessor
    {
        private readonly ILogger logger;

        [NotNull]
        private readonly Dictionary<string, FieldInfo> fieldInfos;

        public DataHandlerDictionaryAccessor(ILogger logger)
        {
            this.logger = logger;
            fieldInfos = FindDictionaryFieldList();
        }
        
        public Dictionary<string, T> GetDictionary<T>([NotNull] string dictionaryName)
        {
            if (!fieldInfos.TryGetValue(dictionaryName, out FieldInfo field))
            {
                logger.Error($"Cannot find data dictionary {dictionaryName}.");
                return null;
            }
                
            return GetDictionaryFromFieldInfo<T>(field);
        }

        [NotNull]
        private static Dictionary<string, FieldInfo> FindDictionaryFieldList()
        {
            var dictionaryField = new Dictionary<string, FieldInfo>();
            var fieldList = typeof(DataHandler).GetFields();
            foreach (FieldInfo field in fieldList)
            {
                if (!typeof(IDictionary).IsAssignableFrom(field.FieldType))
                {
                    continue;
                }

                dictionaryField.Add(field.Name, field);
            }

            return dictionaryField;
        }
        
        private Dictionary<string, T> GetDictionaryFromFieldInfo<T>([NotNull] FieldInfo field)
        {
            return (Dictionary<string, T>) field.GetValue(null);
        }

    }
}