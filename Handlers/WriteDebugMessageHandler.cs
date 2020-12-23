using InteractionsPlus.UI.IMGUI;

namespace InteractionsPlus.Handlers
{
    public class WriteDebugMessageHandler
    {
        [InteractionHandler]
        public static void WriteDebug(string message)
        {
            if (!InteractionsPlusMod.Services.TryResolve(out IMGUIExecutor executor) || executor == null)
            {
                return;
            }
            executor.AddHandler(new DebugMessageIMGUI(executor, message));
        }
    }
}