using System;

namespace InteractionsPlus
{
    public interface ILogger
    {
        void Log(string msg);
        void Critical(string msg);
        void Error(string msg);
        void Warning(string msg);
        void LogException(Exception exception);
    }
}