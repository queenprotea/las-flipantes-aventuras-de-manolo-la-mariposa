using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RoomsScreen : Screen
    {
        internal RoomsScreen()
            : base(TextKeys.Rooms.HeaderLabel, true)
        {
            
        }

        protected override Widget Build()
        {
            VerticalStackPanel panel =  new VerticalStackPanel();
            panel.Widgets.Add(BuildSearchRoom());
            
            return panel;
        }

        private Widget BuildSearchRoom()
        {
            HorizontalStackPanel row = new HorizontalStackPanel();
            row.Widgets.Add(new LabeledTextBox( TextKeys.Rooms.SearchPlaceholder, false) );

            return row;
        }
    }
}

