using UnityEngine;

namespace InteractionsPlus.UI.IMGUI
{
    public class DebugMessageIMGUI : IIMGUIHandler
    {
        private readonly IMGUIExecutor executor;
        private readonly string message;
        private float timeout;
        public DebugMessageIMGUI(IMGUIExecutor executor, string message)
        {
            this.executor = executor;
            this.message = message;
            timeout = 5f;
        }

        public void OnGUI()
        {
            
            timeout -= Time.unscaledDeltaTime;
            if (timeout <= 0f)
            {
                executor.RemoveHandler(this);
                return;
            }
            
            GUILayout.Label(message);
        }
    }
}