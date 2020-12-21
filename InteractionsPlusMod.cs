using InteractionsPlus.UI.IMGUI;

namespace InteractionsPlus
{
    public class InteractionsPlusMod
    {
        private readonly ILogger logger;
        public readonly IMGUIExecutor IMGUIExecutor;
        
        public InteractionsPlusMod(ILogger logger)
        {
            this.logger = logger;
            IMGUIExecutor = new IMGUIExecutor();
        }
        
        public bool OnToggle(bool toggle)
        {
            if (toggle)
            {
                EnableMod();
            }
            else
            {
                DisableMod();
            }
            return true;
        }
        
        private void EnableMod()
        {
            var enabledMessageUI = new ModEnabledIMGUI(logger, IMGUIExecutor);
            IMGUIExecutor.AddHandler(enabledMessageUI);
            logger.Log("Interactions+ enabled");
        }

        private void DisableMod()
        {
            logger.Log("Interactions+ disabled");
        }
    }
}