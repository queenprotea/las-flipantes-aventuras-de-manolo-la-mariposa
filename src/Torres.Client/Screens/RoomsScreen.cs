using Myra.Events;
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
            VerticalStackPanel panel =  Page();
            panel.Spacing = Theme.FieldSpacing;
            panel.Widgets.Add(BuildSearchRoom());
            panel.Widgets.Add(BuildAvaliableRoomsHeader());
            
            return panel;
        }

        private Widget BuildSearchRoom()
        {
            HorizontalStackPanel row = Row();
            
            var searchBox = new LabeledTextBox(TextKeys.Rooms.SearchPlaceholder, false);
            searchBox.VerticalAlignment = VerticalAlignment.Center;
            
            LocalizedButton searchButton = SecondaryButton(TextKeys.Rooms.SearchButton);
            searchButton.Click += OnSearchButtonClicked;
            searchButton.VerticalAlignment = VerticalAlignment.Bottom;
            
            LocalizedButton createRoomButton = PrimaryButton(TextKeys.Rooms.CreateRoomButton);
            createRoomButton.Click += OnCreateRoomButtonClicked;
            createRoomButton.VerticalAlignment = VerticalAlignment.Bottom;
            
            row.Widgets.Add(searchBox);
            row.Widgets.Add(searchButton);
            row.Widgets.Add(createRoomButton);
            
            StackPanel.SetProportionType(searchBox, ProportionType.Fill);
            
            return row;
        }

        private HorizontalStackPanel BuildAvaliableRoomsHeader()
        {
            var row = new HorizontalStackPanel();
            
            LocalizedButton refreshButton = SecondaryButton(TextKeys.Rooms.RefreshButton);
            refreshButton.Click += OnRefreshButtonClicked;
            refreshButton.VerticalAlignment = VerticalAlignment.Bottom;
            refreshButton.HorizontalAlignment = HorizontalAlignment.Right;

            var onlyPublic = Title(TextKeys.Rooms.PublicOnlyBadge);
            onlyPublic.VerticalAlignment = VerticalAlignment.Bottom;

            var avaliableRooms = Title(TextKeys.Rooms.AvailableRoomsTitle);
            avaliableRooms.VerticalAlignment = VerticalAlignment.Bottom;
            
            row.Widgets.Add(avaliableRooms);
            row.Widgets.Add(onlyPublic);
            row.Widgets.Add(refreshButton);
       
            StackPanel.SetProportionType(onlyPublic, ProportionType.Fill);
            
            return row;
        }

        private VerticalStackPanel BuildAvaliableRoomsFooter()
        {
            var row = new VerticalStackPanel();
            
            
            
        }
        private void OnSearchButtonClicked(object sender, MyraEventArgs arguments)
        {
            //TODO: Implement search functionality
        }
        
        private void OnCreateRoomButtonClicked(object sender, MyraEventArgs arguments)
        {
            //TODO: Implement create room functionality
        }

        private void OnRefreshButtonClicked(object sender, MyraEventArgs arguments)
        {
            //TODO: Implement refresh avaliable rooms functionality
        }

        private VerticalStackPanel FillRoomsList(Room[] rooms)
        {
            var avalaibleRooms = new VerticalStackPanel();
            
            if (rooms.Length == 0)
            {
                return avalaibleRooms;
            }
            
            foreach (var Room in Rooms)
            {
                var row = new HorizontalStackPanel();
                row = buildRoomRow(Room);
                avalaibleRooms.Widgets.Add(row);
            }

            return avalaibleRooms;
        }

        private HorizontalStackPanel buildRoomRow(Room Room)
        {
            var row = new 
        }
    }
}

