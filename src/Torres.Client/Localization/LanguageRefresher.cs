using System;
using System.Collections.Generic;

using Myra.Graphics2D.UI;

namespace Torres.Client.Localization
{
    internal static class LanguageRefresher
    {
        internal static int Refresh(Widget root)
        {
            ArgumentNullException.ThrowIfNull(root);

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

        private static IEnumerable<Widget> Children(Widget widget)
        {
            if (widget is Container container)
            {
                return container.Widgets;
            }

            if ((widget is ContentControl content) && (content.Content is not null))
            {
                return new[] { content.Content };
            }

            return Array.Empty<Widget>();
        }
    }
}
