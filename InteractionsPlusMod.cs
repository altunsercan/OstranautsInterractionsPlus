using System.Reflection;
using HarmonyLib;
using InteractionsPlus.Handlers;
using InteractionsPlus.UI.IMGUI;
using JetBrains.Annotations;

namespace InteractionsPlus
{
    public class InteractionsPlusMod
    {
        [NotNull] public static readonly ServiceLocator Services = new ServiceLocator(); 
        
        [NotNull] private readonly ILogger logger;
        [NotNull] private readonly Harmony harmony;
        
        [NotNull] private readonly IMGUIExecutor immediateGUIExecutor;
        [NotNull] private readonly HandlerManager handlerManager;
        [NotNull] private readonly InteractionAdditionalActionsManager additionalDataManager;
        
        internal InteractionsPlusMod([NotNull]ILogger logger)
        {
            this.logger = logger;
            handlerManager = new HandlerManager(logger);
            additionalDataManager = new InteractionAdditionalActionsManager();
            immediateGUIExecutor = new IMGUIExecutor();
            harmony = new Harmony("com.ostranauts.marchingninja.interactionsplus");

            Services.SetLogger(logger);
            Services.Bind<ILogger>(logger);
            Services.Bind<HandlerManager>(handlerManager);
            Services.Bind<InteractionAdditionalActionsManager>(additionalDataManager);
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
            handlerManager.DisoverHandlersInAllAssemblies();
            
            harmony.PatchAll(Assembly.GetExecutingAssembly());
            logger.Log("Interactions+ enabled");
        }

        private void DisableMod()
        {
            harmony.UnpatchAll();
            logger.Log("Interactions+ disabled");
        }
    }
}