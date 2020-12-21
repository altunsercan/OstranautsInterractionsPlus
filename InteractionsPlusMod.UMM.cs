using System;
using UnityModManagerNet;

namespace InteractionsPlus
{
    public static class InteractionsPlusModUMM
    {
        private static InteractionsPlusMod Instance;
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            if (Instance != null)
            {
                return true;
            }
            
            var logger = new UMMLoggerAdapter(modEntry.Logger);
            
            Instance = new InteractionsPlusMod(logger);
            modEntry.OnToggle += (entry, toggle) =>  Instance.OnToggle(toggle);
            modEntry.OnFixedGUI += OnGUI;
            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            modEntry.Logger.Log($"Update: {Instance!=null}");
            Instance?.IMGUIExecutor.Update();
        }

        private class UMMLoggerAdapter : ILogger
        {
            private readonly UnityModManager.ModEntry.ModLogger modEntryLogger;

            public UMMLoggerAdapter(UnityModManager.ModEntry.ModLogger modEntryLogger)
            {
                this.modEntryLogger = modEntryLogger;
            }

            public void Log(string msg) => modEntryLogger.Log(msg);
            public void Critical(string msg) => modEntryLogger.Critical(msg);
            public void Error(string msg) => modEntryLogger.Error(msg);
            public void Warning(string msg) => modEntryLogger.Warning(msg);
            public void LogException(Exception exception) => modEntryLogger.LogException(exception);
        }
    }
}