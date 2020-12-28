using System;

namespace InteractionsPlus.Exceptions
{
    public class JsonFormatException : Exception
    {
        public JsonFormatException(Exception innerException, string key, Type jsonType):base(CreateMessage(key, jsonType), innerException)
        {
        }

        private static string CreateMessage(string key, Type jsonType)
        {
            return $"Cannot convert {key} to expected type {jsonType.Name}";
        }

        public override string ToString()
        {
            var message = base.ToString();
            message += "\n === Inner Exception === \n";
            message += InnerException.ToString();
            return message;
        }
    }
}