using InteractionsPlus.UI.IMGUI;

namespace InteractionsPlus.Handlers
{
    public class WriteDebugMessageHandler
    {
        [InteractionHandler]
        public static bool WriteDebug(InteractionTriggerArgs args, string message)
        {
            if (!InteractionsPlusMod.Services.TryResolve(out IMGUIExecutor executor) || executor == null)
            {
                return false;
            }

            if (args.IgnoreItems) // set true during dryrun
            {
                return true;
            }
            
            executor.AddHandler(new DebugMessageIMGUI(executor, message));
            return true;
        }
    }
}