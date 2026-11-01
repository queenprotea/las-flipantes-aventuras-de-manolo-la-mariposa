using System;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class CreateRoomScreen : Screen
    {
        private const int CardWidth = 460;
        private const int OptionCardWidth = 200;

        private readonly VerticalStackPanel _visibilityOptionsPlaceholder = new VerticalStackPanel { Spacing = Metrics.FieldSpacing };
        private readonly NoticeBox _noticeBox = new NoticeBox();
        private readonly LocalizedButton _createButton = CreatePrimaryButton(TextKeys.CreateRoom.CreateRoomButton);
        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _session;

        private bool _isPublic = true;
        private Task<CreateRoomResult>? _creationRequest;

        internal CreateRoomScreen(GameSessionConnection gameSessionConnection, PlayerSession session)
            : base(TextKeys.CreateRoom.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(session);

            _gameSessionConnection = gameSessionConnection;
            _session = session;
        }

        internal override void Open()
        {
            _noticeBox.Hide();
            if (!_session.IsLoggedIn || !_gameSessionConnection.IsOpen)
            {
                RequestedScreen = ScreenId.Login;
            }
            else if (_session.CurrentRoom is not null)
            {
                RequestedScreen = ScreenId.Room;
            }
        }

        internal override void Update(GameTime gameTime)
        {
            if ((_creationRequest is null) || !_creationRequest.IsCompleted)
            {
                return;
            }

            Task<CreateRoomResult> completedCreation = _creationRequest;
            _creationRequest = null;
            _createButton.Enabled = true;
            if (completedCreation.IsCompletedSuccessfully)
            {
                ShowCreationOutcome(completedCreation.Result);
            }
            else
            {
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
            }
        }

        protected override Widget Build()
        {
            VerticalStackPanel card = CreateCard(CardWidth);
            card.VerticalAlignment = VerticalAlignment.Center;
            card.Widgets.Add(CreateCardHeader(TextKeys.CreateRoom.Title, TextKeys.CreateRoom.Hint));
            card.Widgets.Add(_visibilityOptionsPlaceholder);
            RenderVisibilityOptions();
            card.Widgets.Add(CreateParagraph(TextKeys.CreateRoom.SharedRulesHint));
            card.Widgets.Add(_noticeBox);
            card.Widgets.Add(BuildActionsRow());

            HorizontalStackPanel columns = CreateColumns();
            columns.Widgets.Add(card);
            columns.Widgets.Add(CreateAmbiencePanel());

            VerticalStackPanel page = CreatePage();
            page.Widgets.Add(columns);
            StackPanel.SetProportionType(columns, ProportionType.Fill);

            return page;
        }

        private void RenderVisibilityOptions()
        {
            LocalizedButton publicOptionButton = BuildOptionButton(TextKeys.CreateRoom.PublicOption, _isPublic);
            LocalizedButton privateOptionButton = BuildOptionButton(TextKeys.CreateRoom.PrivateOption, !_isPublic);
            publicOptionButton.Click += PublicOptionButtonOnClick;
            privateOptionButton.Click += PrivateOptionButtonOnClick;

            HorizontalStackPanel optionsRow = CreateButtonRow();
            optionsRow.Widgets.Add(BuildOptionCard(publicOptionButton, TextKeys.CreateRoom.PublicOptionHint, _isPublic));
            optionsRow.Widgets.Add(BuildOptionCard(privateOptionButton, TextKeys.CreateRoom.PrivateOptionHint, !_isPublic));

            _visibilityOptionsPlaceholder.Widgets.Clear();
            _visibilityOptionsPlaceholder.Widgets.Add(optionsRow);
        }

        private void SelectVisibility(bool isPublic)
        {
            _isPublic = isPublic;
            RenderVisibilityOptions();
        }

        private static LocalizedButton BuildOptionButton(string titleKey, bool isSelected)
        {
            LocalizedButton optionButton = isSelected ? CreatePrimaryButton(titleKey) : CreateSecondaryButton(titleKey);
            optionButton.HorizontalAlignment = HorizontalAlignment.Stretch;

            return optionButton;
        }

        private static Panel BuildOptionCard(LocalizedButton optionButton, string hintKey, bool isSelected)
        {
            var optionContent = new VerticalStackPanel { Spacing = Metrics.CardHeaderSpacing };
            optionContent.Widgets.Add(optionButton);
            optionContent.Widgets.Add(CreateHint(hintKey));

            var optionCard = new Panel
            {
                Width = OptionCardWidth,
                Padding = Metrics.CardPadding,
                Background = isSelected ? Theme.MintTintBrush : Theme.SurfaceBrush,
                Border = isSelected ? Theme.MintLineBrush : Theme.LineBrush,
                BorderThickness = Sizes.Border,
            };
            optionCard.Widgets.Add(optionContent);

            return optionCard;
        }

        private HorizontalStackPanel BuildActionsRow()
        {
            HorizontalStackPanel actionsRow = CreateButtonRow();

            _createButton.Click += CreateButtonOnClick;
            actionsRow.Widgets.Add(_createButton);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackButton);
            backButton.Click += BackButtonOnClick;
            actionsRow.Widgets.Add(backButton);

            return actionsRow;
        }

        private void PublicOptionButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectVisibility(true);
        }

        private void PrivateOptionButtonOnClick(object sender, MyraEventArgs e)
        {
            SelectVisibility(false);
        }

        private void ShowCreationOutcome(CreateRoomResult creation)
        {
            switch (creation.Status)
            {
                case CreateRoomStatus.Created:
                    EnterRoom(creation.Room);
                    break;

                case CreateRoomStatus.AlreadyInRoom:
                    _noticeBox.ShowTitle(TextKeys.Rooms.AlreadyInRoomError);
                    break;

                case CreateRoomStatus.Banned:
                    _noticeBox.ShowBan(creation.BanEndsAtUtc);
                    break;

                case CreateRoomStatus.NotIdentified:
                    RequestedScreen = ScreenId.Login;
                    break;

                case CreateRoomStatus.InvalidVisibility:
                case CreateRoomStatus.DatabaseUnavailable:
                    _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
                    break;

                default:
                    throw new ArgumentOutOfRangeException(nameof(creation), creation.Status, null);
            }
        }

        private void EnterRoom(Room? createdRoom)
        {
            if (createdRoom is null)
            {
                _noticeBox.ShowMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);
                return;
            }

            _session.EnterRoom(createdRoom);
            RequestedScreen = ScreenId.Room;
        }

        private void CreateButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_creationRequest is not null)
            {
                return;
            }

            _noticeBox.Hide();
            _createButton.Enabled = false;
            RoomVisibility visibility = _isPublic ? RoomVisibility.Public : RoomVisibility.Private;
            _creationRequest = _gameSessionConnection.CreateRoomAsync(visibility);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.Rooms;
        }
    }
}
