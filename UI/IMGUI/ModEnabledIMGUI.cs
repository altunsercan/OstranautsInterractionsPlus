using UnityEngine;

namespace InteractionsPlus.UI.IMGUI
{
    public class ModEnabledIMGUI : IIMGUIHandler
    {
        private readonly ILogger logger;
        private bool firstDraw;
        
        private readonly IMGUIExecutor executor;
        private float timeout;
        public ModEnabledIMGUI(ILogger logger, IMGUIExecutor executor)
        {
            this.logger = logger;
            this.executor = executor;
            timeout = 5f;
        }

        public void OnGUI()
        {
            if (!firstDraw)
            {
                logger.Log("First Draw");
                firstDraw = true;
            }
            
            timeout -= Time.unscaledDeltaTime;
            if (timeout <= 0f)
            {
                executor.RemoveHandler(this);
                return;
            }
            
            GUILayout.Label("Interactions + Loaded");
        }
    }
}