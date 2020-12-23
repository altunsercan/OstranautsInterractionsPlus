using InteractionsPlus.UI.IMGUI;
using JetBrains.Annotations;

namespace InteractionsPlus
{
    public class InteractionsPlusMod
    {
        [NotNull] public static readonly ServiceLocator Services = new ServiceLocator(); 
        
        [NotNull] private readonly ILogger logger;
        
        [NotNull] private readonly IMGUIExecutor immediateGUIExecutor;
        
        internal InteractionsPlusMod([NotNull]ILogger logger)
        {
            this.logger = logger;
            IMGUIExecutor = new IMGUIExecutor();
            immediateGUIExecutor = new IMGUIExecutor();
            Services.SetLogger(logger);
            Services.Bind<ILogger>(logger);
            Services.Bind<IMGUIExecutor>(immediateGUIExecutor);
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