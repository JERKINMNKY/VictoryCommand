using System.Collections.Generic;

namespace IFC.Systems.UI
{
    public interface IUIRefreshable
    {
        void Refresh();
    }

    public static class UIRefreshService
    {
        private static readonly List<IUIRefreshable> Refreshables = new List<IUIRefreshable>();

        public static void Register(IUIRefreshable refreshable)
        {
            if (refreshable != null && !Refreshables.Contains(refreshable))
            {
                Refreshables.Add(refreshable);
            }
        }

        public static void Unregister(IUIRefreshable refreshable)
        {
            if (refreshable != null)
            {
                Refreshables.Remove(refreshable);
            }
        }

        public static void RefreshAll()
        {
            for (int i = 0; i < Refreshables.Count; i++)
            {
                Refreshables[i]?.Refresh();
            }
        }
    }
}
