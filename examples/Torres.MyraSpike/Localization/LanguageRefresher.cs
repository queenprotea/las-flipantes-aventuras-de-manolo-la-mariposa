using Myra.Graphics2D.UI;

namespace Torres.MyraSpike.Localization
{
    internal static class LanguageRefresher
    {
        internal static int Refresh(Widget root)
        {
            int refreshed = 0;
            if (root is ILocalizedWidget localized)
            {
                localized.RefreshText();
                refreshed += 1;
            }

            foreach (Widget child in Children(root))
            {
                refreshed += Refresh(child);
            }

            return refreshed;
        }

        private static System.Collections.Generic.IEnumerable<Widget> Children(Widget widget)
        {
            if (widget is Container container)
            {
                return container.Widgets;
            }

            if ((widget is ContentControl content) && (content.Content is not null))
            {
                return new[] { content.Content };
            }

            return System.Array.Empty<Widget>();
        }
    }
}
