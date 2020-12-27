using System;
using InteractionsPlus.ModDependency;
using InteractionsPlus.UI.IMGUI;
using JetBrains.Annotations;
using UnityModManagerNet;

namespace InteractionsPlus
{
    public static class InteractionsPlusModUMM
    {
        private static InteractionsPlusMod Instance;

        private static IMGUIExecutor guiExecutor;
        
        private static bool Load(UnityModManager.ModEntry modEntry)
        {
            if (Instance != null)
            {
                return true;
            }

            if (modEntry == null)
            {
                return false;
            }
            
            
            ILogger logger = modEntry.Logger != null ? (ILogger) new UMMLoggerAdapter(modEntry.Logger) : new NullLogger();

            Instance = new InteractionsPlusMod(logger);
            modEntry.OnToggle += (entry, toggle) =>  Instance.OnToggle(toggle);
            modEntry.OnFixedGUI += OnGUI;

            if (InteractionsPlusMod.Services.TryResolve(out IMGUIExecutor executor))
            {
                guiExecutor = executor;
            }

            if (InteractionsPlusMod.Services.TryResolve(out UMMDependencyManager dependencyManager))
            {
                dependencyManager.SetModList(UnityModManager.modEntries);
            }

            return true;
        }

        private static void OnGUI(UnityModManager.ModEntry modEntry)
        {
            guiExecutor?.Update();
        }

        private class UMMLoggerAdapter : ILogger
        {
            [NotNull]
            private readonly UnityModManager.ModEntry.ModLogger modEntryLogger;

            public UMMLoggerAdapter([NotNull] UnityModManager.ModEntry.ModLogger modEntryLogger)
            {
                this.modEntryLogger = modEntryLogger;
            }

            public void Log(string msg) => modEntryLogger.Log(msg);
            public void Critical(string msg) => modEntryLogger.Critical(msg);
            public void Error(string msg) => modEntryLogger.Error(msg);
            public void Warning(string msg) => modEntryLogger.Warning(msg);
            public void LogException(Exception exception) => modEntryLogger.LogException(exception);
        }

        private class NullLogger : ILogger
        {
            public void Log(string msg) { }
            public void Critical(string msg) { }
            public void Error(string msg) { }
            public void Warning(string msg) { }
            public void LogException(Exception exception) { }
        }
    }
}