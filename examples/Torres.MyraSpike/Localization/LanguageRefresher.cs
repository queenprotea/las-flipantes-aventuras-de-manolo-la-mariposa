using System;
using System.Collections.Generic;

using Myra.Graphics2D.UI;

namespace Torres.MyraSpike.Localization
{
    internal static class LanguageRefresher
    {
        internal static int Refresh(Widget rootWidget)
        {
            int refreshedWidgetCount = 0;
            if (rootWidget is ILocalizedWidget localizedWidget)
            {
                localizedWidget.RefreshText();
                refreshedWidgetCount += 1;
            }

            foreach (Widget childWidget in GetChildWidgets(rootWidget))
            {
                refreshedWidgetCount += Refresh(childWidget);
            }

            return refreshedWidgetCount;
        }

        private static IEnumerable<Widget> GetChildWidgets(Widget parentWidget)
        {
            IEnumerable<Widget> childWidgets = Array.Empty<Widget>();
            if (parentWidget is Container parentContainer)
            {
                childWidgets = parentContainer.Widgets;
            }
            else if ((parentWidget is ContentControl contentControl) && (contentControl.Content is not null))
            {
                childWidgets = new[] { contentControl.Content };
            }

            return childWidgets;
        }
    }
}
