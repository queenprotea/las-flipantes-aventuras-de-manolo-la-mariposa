using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class RoomsScreen : Screen
    {
        private const int EmptyStateSpacing = 4;

        internal RoomsScreen()
            : base(TextKeys.Rooms.HeaderLabel, true)
        {
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = Page();
            page.Spacing = Theme.FieldSpacing;
            page.Widgets.Add(BuildSearchRow());
            page.Widgets.Add(BuildAvailableRoomsHeader());

            page.Widgets.Add(BuildEmptyState());
            page.Widgets.Add(Hint(TextKeys.Rooms.PrivateRoomsHint));

            var filler = new Panel();
            page.Widgets.Add(filler);
            StackPanel.SetProportionType(filler, ProportionType.Fill);

            LocalizedButton backButton = SecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += OnBackClick;
            page.Widgets.Add(BackBar(backButton));
            return page;
        }

        private static HorizontalStackPanel BuildSearchRow()
        {
            HorizontalStackPanel row = Row();

            var searchField = new LabeledTextBox(TextKeys.Rooms.SearchPlaceholder, false);
            LocalizedButton searchButton = SecondaryButton(TextKeys.Rooms.SearchButton);
            searchButton.VerticalAlignment = VerticalAlignment.Bottom;
            LocalizedButton createRoomButton = PrimaryButton(TextKeys.Rooms.CreateRoomButton);
            createRoomButton.VerticalAlignment = VerticalAlignment.Bottom;

            row.Widgets.Add(searchField);
            row.Widgets.Add(searchButton);
            row.Widgets.Add(createRoomButton);
            StackPanel.SetProportionType(searchField, ProportionType.Fill);
            return row;
        }

        private static HorizontalStackPanel BuildAvailableRoomsHeader()
        {
            HorizontalStackPanel row = Row();

            LocalizedLabel title = Title(TextKeys.Rooms.AvailableRoomsTitle);
            title.VerticalAlignment = VerticalAlignment.Center;

            Panel publicOnlyChip = BuildChip(TextKeys.Rooms.PublicOnlyBadge);

            LocalizedButton refreshButton = SmallSecondaryButton(TextKeys.Rooms.RefreshButton);
            refreshButton.VerticalAlignment = VerticalAlignment.Center;

            var spacer = new Panel();
            row.Widgets.Add(title);
            row.Widgets.Add(publicOnlyChip);
            row.Widgets.Add(spacer);
            row.Widgets.Add(refreshButton);
            StackPanel.SetProportionType(spacer, ProportionType.Fill);
            return row;
        }

        private static Panel BuildChip(string textKey)
        {
            var chip = new Panel
            {
                Padding = Theme.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Theme.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            chip.Widgets.Add(new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
            });
            return chip;
        }

        private static Panel BuildEmptyState()
        {
            var emptyState = new VerticalStackPanel
            {
                Spacing = EmptyStateSpacing,
                HorizontalAlignment = HorizontalAlignment.Center,
                VerticalAlignment = VerticalAlignment.Center,
            };

            var title = new LocalizedLabel(TextKeys.Rooms.EmptyStateTitle)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                HorizontalAlignment = HorizontalAlignment.Center,
            };
            LocalizedLabel hint = Hint(TextKeys.Rooms.EmptyStateHint);
            hint.HorizontalAlignment = HorizontalAlignment.Center;
            emptyState.Widgets.Add(title);
            emptyState.Widgets.Add(hint);

            var frame = new Panel
            {
                Padding = Theme.CardPadding,
                Background = Theme.SurfaceSunkenBrush,
                Border = Theme.LineBrush,
                BorderThickness = Theme.Border,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            frame.Widgets.Add(emptyState);
            return frame;
        }

        private void OnBackClick(object sender, MyraEventArgs arguments)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
