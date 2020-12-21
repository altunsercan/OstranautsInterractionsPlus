using System.Collections.Generic;
using InteractionsPlus.JetBrains.Annotations;

namespace InteractionsPlus.UI.IMGUI
{
    public class IMGUIExecutor
    {
        [NotNull, ItemNotNull]
        private readonly List<IIMGUIHandler> imguiHandler = new List<IIMGUIHandler>();

        public void AddHandler(IIMGUIHandler handler)
        {
            imguiHandler.Add(handler);
        }

        public void RemoveHandler(IIMGUIHandler handler)
        {
            imguiHandler.Remove(handler);
        }
        
        public void Update()
        {
            foreach (IIMGUIHandler handler in imguiHandler)
            {
                handler.OnGUI();
            }
        }
    }
}