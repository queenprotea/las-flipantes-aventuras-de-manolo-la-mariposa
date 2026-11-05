using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;

using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Events;
using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Session;
using Torres.Client.Ui;

namespace Torres.Client.Screens
{
    internal sealed class HistoryScreen : Screen
    {
        internal const int SingleGuest = 1;

        private const int DateColumnWidth = 140;
        private const int PlayersColumnWidth = 90;
        private const int StatColumnWidth = 90;
        private const int ResultColumnWidth = 170;

        private const int NoThickness = 0;
        private const int FirstRankPosition = 1;
        private const int ResultTagSpacing = 4;
        private const int SinglePage = 1;
        private const int PageStep = 1;
        private const int CallerSeatCount = 1;
        private const int NoGuests = 0;

        private const int ColumnCount = 6;
        private const int ColumnIndexDate = 0;
        private const int ColumnIndexPlayers = 1;
        private const int ColumnIndexOpponents = 2;
        private const int ColumnIndexRank = 3;
        private const int ColumnIndexPoints = 4;
        private const int ColumnIndexResult = 5;

        private const int HeaderRowIndex = 0;

        private const string DateFormat = "dd MMM yyyy";
        private const string OpponentsWithGuestsFormat = "{0} {1}";

        private static readonly ChipStyle _neutralChipStyle = new ChipStyle(Theme.SurfaceAltBrush, Theme.LineBrush, Theme.MutedInk);
        private static readonly ChipStyle _blushChipStyle = new ChipStyle(Theme.BlushTintBrush, Theme.BlushBrush, Theme.BlushInk);

        private readonly GameSessionConnection _gameSessionConnection;
        private readonly PlayerSession _playerSession;
        private readonly LocalizedLabel _loadingLabel;
        private readonly VerticalStackPanel _contentPlaceholder = new VerticalStackPanel { Visible = false };
        private readonly VerticalStackPanel _failureNotice;
        private readonly HorizontalStackPanel _pageBar;
        private readonly LocalizedButton _previousPageButton;
        private readonly LocalizedButton _nextPageButton;
        private readonly ComputedLabel _pageLabel;
        private readonly Dictionary<Button, int> _matchIdsByRowClickTarget = new Dictionary<Button, int>();

        private Task<MatchHistoryResponse>? _historyRequest;
        private int _requestedPageNumber = MatchHistorySelection.FirstPageNumber;
        private int _shownPageNumber = MatchHistorySelection.FirstPageNumber;
        private int _pageCount;

        internal HistoryScreen(GameSessionConnection gameSessionConnection, PlayerSession playerSession)
            : base(TextKeys.History.HeaderLabel, true)
        {
            ArgumentNullException.ThrowIfNull(gameSessionConnection);
            ArgumentNullException.ThrowIfNull(playerSession);

            _gameSessionConnection = gameSessionConnection;
            _playerSession = playerSession;
            _loadingLabel = CreateHint(TextKeys.History.LoadingLabel);

            LocalizedButton retryButton = CreatePrimaryButton(TextKeys.Common.RetryButton);
            retryButton.Click += RetryButtonOnClick;
            _failureNotice = CreateFailureNotice(retryButton);

            _pageLabel = new ComputedLabel(FormatPageLabel)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                VerticalAlignment = VerticalAlignment.Center,
            };
            _previousPageButton = CreateSmallSecondaryButton(TextKeys.History.PreviousPageButton);
            _previousPageButton.Click += PreviousPageButtonOnClick;
            _nextPageButton = CreateSmallSecondaryButton(TextKeys.History.NextPageButton);
            _nextPageButton.Click += NextPageButtonOnClick;
            _pageBar = BuildPageBar();
        }

        internal override void Open()
        {
            _historyRequest = null;
            _pageCount = 0;
            _contentPlaceholder.Widgets.Clear();
            _contentPlaceholder.Visible = false;
            _pageBar.Visible = false;
            _failureNotice.Visible = false;

            int pageToOpen = _playerSession.MatchHistory.TakePageToResume();
            if (!_playerSession.HasAccount || !_gameSessionConnection.IsSessionOpen)
            {
                RequestedScreen = ScreenId.Login;
                return;
            }

            StartHistoryRequest(pageToOpen);
        }

        internal override void Update(GameTime gameTime)
        {
            if ((_historyRequest is null) || !_historyRequest.IsCompleted)
            {
                return;
            }

            Task<MatchHistoryResponse> completedRequest = _historyRequest;
            _historyRequest = null;
            _loadingLabel.Visible = false;
            if (completedRequest.IsCompletedSuccessfully)
            {
                ShowPage(completedRequest.Result);
            }
            else
            {
                ShowFailure();
            }
        }

        protected override Widget Build()
        {
            VerticalStackPanel page = CreatePage();
            page.Widgets.Add(_loadingLabel);
            page.Widgets.Add(_contentPlaceholder);
            StackPanel.SetProportionType(_contentPlaceholder, ProportionType.Fill);
            page.Widgets.Add(_pageBar);
            page.Widgets.Add(_failureNotice);

            LocalizedButton backButton = CreateSecondaryButton(TextKeys.Common.BackToMenuButton);
            backButton.Click += BackButtonOnClick;
            page.Widgets.Add(CreateBackBar(backButton));

            return page;
        }

        private void StartHistoryRequest(int pageNumber)
        {
            _requestedPageNumber = pageNumber;
            _failureNotice.Visible = false;

            if (!_gameSessionConnection.IsSessionOpen)
            {
                _loadingLabel.Visible = false;
                ShowFailure();
                return;
            }

            _loadingLabel.Visible = true;
            _historyRequest = _gameSessionConnection.GetMatchHistoryAsync(pageNumber);
            UpdatePageButtons();
        }

        private void ShowPage(MatchHistoryResponse historyPage)
        {
            _shownPageNumber = historyPage.PageNumber;
            _pageCount = historyPage.PageCount;
            _contentPlaceholder.Widgets.Clear();
            _matchIdsByRowClickTarget.Clear();

            Widget historyContent = historyPage.Entries.Count == 0
                ? CreateEmptyState(TextKeys.History.EmptyStateTitle, TextKeys.History.EmptyStateHint)
                : BuildTableSection(historyPage);
            _contentPlaceholder.Widgets.Add(historyContent);
            StackPanel.SetProportionType(historyContent, ProportionType.Fill);
            _contentPlaceholder.Visible = true;

            _pageLabel.RefreshText();
            _pageBar.Visible = _pageCount > SinglePage;
            UpdatePageButtons();
        }

        private void ShowFailure()
        {
            _contentPlaceholder.Visible = false;
            _pageBar.Visible = false;
            _failureNotice.Visible = true;
        }

        private void UpdatePageButtons()
        {
            bool isIdle = _historyRequest is null;
            _previousPageButton.Enabled = isIdle && (_shownPageNumber > MatchHistorySelection.FirstPageNumber);
            _nextPageButton.Enabled = isIdle && (_shownPageNumber < _pageCount);
        }

        private HorizontalStackPanel BuildPageBar()
        {
            HorizontalStackPanel pageBar = CreateButtonRow();
            pageBar.Visible = false;
            pageBar.Widgets.Add(_previousPageButton);
            pageBar.Widgets.Add(_pageLabel);
            pageBar.Widgets.Add(_nextPageButton);

            return pageBar;
        }

        private string FormatPageLabel()
        {
            string pageText = LocalizedText.Format(
                TextKeys.History.PageLabel,
                DisplayFormats.FormatInteger(_shownPageNumber),
                DisplayFormats.FormatInteger(_pageCount));

            return pageText;
        }

        private VerticalStackPanel BuildTableSection(MatchHistoryResponse historyPage)
        {
            Grid headerGrid = CreateTableGrid();
            headerGrid.Margin = new Thickness(NoThickness, NoThickness, Metrics.ScrollbarWidth, NoThickness);
            AddHeaderRow(headerGrid);

            var scrollableBody = new ScrollViewer
            {
                Content = BuildBody(historyPage),
                ShowVerticalScrollBar = true,
                ShowHorizontalScrollBar = false,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            ScrollbarLook.Apply(scrollableBody);

            var tableSection = new VerticalStackPanel
            {
                Spacing = Metrics.FieldSpacing,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            tableSection.Widgets.Add(headerGrid);
            tableSection.Widgets.Add(CreateDivider());
            tableSection.Widgets.Add(scrollableBody);
            StackPanel.SetProportionType(scrollableBody, ProportionType.Fill);

            return tableSection;
        }

        private static Grid CreateTableGrid()
        {
            var tableGrid = new Grid
            {
                ColumnSpacing = Metrics.ColumnSpacing,
                RowSpacing = Metrics.FieldSpacing,
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Top,
            };
            tableGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, DateColumnWidth));
            tableGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, PlayersColumnWidth));
            tableGrid.ColumnsProportions.Add(new Proportion(ProportionType.Fill));
            tableGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, StatColumnWidth));
            tableGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, StatColumnWidth));
            tableGrid.ColumnsProportions.Add(new Proportion(ProportionType.Pixels, ResultColumnWidth));

            return tableGrid;
        }

        private Grid BuildBody(MatchHistoryResponse historyPage)
        {
            Grid bodyGrid = CreateTableGrid();
            for (var matchIndex = 0; matchIndex < historyPage.Entries.Count; matchIndex++)
            {
                bodyGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
                AddMatchRow(bodyGrid, matchIndex, historyPage.Entries[matchIndex]);
            }

            return bodyGrid;
        }

        private static void AddHeaderRow(Grid headerGrid)
        {
            headerGrid.RowsProportions.Add(new Proportion(ProportionType.Auto));
            AddCell(headerGrid, new CellPosition(HeaderRowIndex, ColumnIndexDate), CreateHint(TextKeys.History.DateColumn));
            AddCell(headerGrid, new CellPosition(HeaderRowIndex, ColumnIndexPlayers), CreateHint(TextKeys.History.PlayersColumn));
            AddCell(headerGrid, new CellPosition(HeaderRowIndex, ColumnIndexOpponents), CreateHint(TextKeys.History.OpponentsColumn));
            AddCell(headerGrid, new CellPosition(HeaderRowIndex, ColumnIndexRank), AlignRight(CreateHint(TextKeys.History.RankColumn)));
            AddCell(headerGrid, new CellPosition(HeaderRowIndex, ColumnIndexPoints), AlignRight(CreateHint(TextKeys.History.PointsColumn)));
            AddCell(headerGrid, new CellPosition(HeaderRowIndex, ColumnIndexResult), CreateHint(TextKeys.History.ResultColumn));
        }

        private void AddMatchRow(Grid historyGrid, int rowIndex, MatchHistoryEntry match)
        {
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexDate), CreateDataLabel(FormatDate(match.PlayedAt)));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexPlayers), CreateDataLabel(DisplayFormats.FormatInteger(match.PlayerCount)));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexOpponents), CreateComputedDataLabel(() => FormatOpponents(match)));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexRank), AlignRight(CreateDataLabel(FormatRank(match.Placement))));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexPoints), AlignRight(CreateDataLabel(FormatPoints(match.Points))));
            AddCell(historyGrid, new CellPosition(rowIndex, ColumnIndexResult), BuildResultCell(match));

            AddRowClickTarget(historyGrid, rowIndex, match.MatchId);
        }

        private void AddRowClickTarget(Grid historyGrid, int rowIndex, int matchId)
        {
            var rowClickTarget = new Button
            {
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(NoThickness),
                Padding = new Thickness(NoThickness),
                HorizontalAlignment = HorizontalAlignment.Stretch,
                VerticalAlignment = VerticalAlignment.Stretch,
            };
            rowClickTarget.Click += RowClickTargetOnClick;
            _matchIdsByRowClickTarget.Add(rowClickTarget, matchId);

            Grid.SetRow(rowClickTarget, rowIndex);
            Grid.SetColumnSpan(rowClickTarget, ColumnCount);
            historyGrid.Widgets.Add(rowClickTarget);
        }

        private static string FormatDate(DateTime playedAt)
        {
            string formattedDate = DisplayFormats.FormatDate(playedAt.ToLocalTime(), DateFormat);
            string lowerCaseDate = formattedDate.ToLower(CultureInfo.CurrentCulture);

            return lowerCaseDate;
        }

        private static string FormatRank(int? rank)
        {
            string rankText = rank.HasValue ? DisplayFormats.FormatRankPosition(rank.Value) : DisplaySymbols.MissingValue;

            return rankText;
        }

        private static string FormatPoints(int? points)
        {
            string pointsText = points.HasValue ? DisplayFormats.FormatThousands(points.Value) : DisplaySymbols.MissingValue;

            return pointsText;
        }

        private static string FormatOpponents(MatchHistoryEntry match)
        {
            string namedOpponents = string.Join(DisplaySymbols.ListSeparator, match.RivalUsernames);
            int guestOpponentCount = Math.Max(NoGuests, match.PlayerCount - CallerSeatCount - match.RivalUsernames.Count);

            if (guestOpponentCount == NoGuests)
            {
                return namedOpponents;
            }

            string guestPhrase = guestOpponentCount == SingleGuest
                ? LocalizedText.Get(TextKeys.History.GuestOpponentMessage)
                : LocalizedText.Format(TextKeys.History.GuestOpponentsMessage, DisplayFormats.FormatInteger(guestOpponentCount));

            string opponentsText = string.IsNullOrEmpty(namedOpponents)
                ? guestPhrase
                : string.Format(CultureInfo.CurrentCulture, OpponentsWithGuestsFormat, namedOpponents, guestPhrase);

            return opponentsText;
        }

        private static VerticalStackPanel BuildResultCell(MatchHistoryEntry match)
        {
            var resultCell = new VerticalStackPanel
            {
                Spacing = ResultTagSpacing,
                VerticalAlignment = VerticalAlignment.Center,
            };
            resultCell.Widgets.Add(BuildResultChip(match));

            if (match.WasWithdrawn)
            {
                resultCell.Widgets.Add(CreateTag(TextKeys.Common.WithdrewTag, Theme.BlushInk));
            }

            return resultCell;
        }

        private static Widget BuildResultChip(MatchHistoryEntry match)
        {
            Widget resultChip = match.Outcome switch
            {
                MatchOutcome.Finished when match.Placement == FirstRankPosition => BuildWinChip(),
                MatchOutcome.Finished => BuildChip(TextKeys.History.FinishedStatus, _neutralChipStyle),
                MatchOutcome.Abandoned => BuildChip(TextKeys.History.ForfeitedStatus, _neutralChipStyle),
                MatchOutcome.Interrupted => BuildChip(TextKeys.History.InterruptedStatus, _blushChipStyle),
                _ => throw new ArgumentOutOfRangeException(nameof(match)),
            };

            return resultChip;
        }

        private static Panel BuildWinChip()
        {
            var winChip = new Panel
            {
                Padding = Metrics.PillButtonPadding,
                Background = Theme.MintTintBrush,
                Border = Theme.MintLineBrush,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };

            HorizontalStackPanel chipContent = CreateButtonRow();
            chipContent.Widgets.Add(new Label { Text = DisplaySymbols.CheckMark, Font = Fonts.Small, TextColor = Theme.MintInk });
            chipContent.Widgets.Add(new LocalizedLabel(TextKeys.History.FinishedStatus) { Font = Fonts.Small, TextColor = Theme.MintInk });
            winChip.Widgets.Add(chipContent);

            return winChip;
        }

        private static Panel BuildChip(string statusTextKey, ChipStyle chipStyle)
        {
            var statusChip = new Panel
            {
                Padding = Metrics.PillButtonPadding,
                Background = chipStyle.Background,
                Border = chipStyle.Border,
                BorderThickness = Sizes.Border,
                HorizontalAlignment = HorizontalAlignment.Left,
                VerticalAlignment = VerticalAlignment.Center,
            };
            statusChip.Widgets.Add(new LocalizedLabel(statusTextKey) { Font = Fonts.Small, TextColor = chipStyle.TextColor });

            return statusChip;
        }

        private static Label CreateDataLabel(string labelText)
        {
            var dataLabel = new Label
            {
                Text = labelText,
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
            };

            return dataLabel;
        }

        private static ComputedLabel CreateComputedDataLabel(Func<string> textProvider)
        {
            var computedDataLabel = new ComputedLabel(textProvider)
            {
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
                SupportsCommands = false,
            };
            computedDataLabel.RefreshText();

            return computedDataLabel;
        }

        private static TLabel AlignRight<TLabel>(TLabel targetLabel)
            where TLabel : Label
        {
            targetLabel.TextAlign = FontStashSharp.RichText.TextHorizontalAlignment.Right;

            return targetLabel;
        }

        private static void AddCell(Grid historyGrid, CellPosition cellPosition, Widget cellContent)
        {
            Grid.SetRow(cellContent, cellPosition.Row);
            Grid.SetColumn(cellContent, cellPosition.Column);
            historyGrid.Widgets.Add(cellContent);
        }

        private void RowClickTargetOnClick(object sender, MyraEventArgs e)
        {
            if ((sender is Button rowClickTarget) && _matchIdsByRowClickTarget.TryGetValue(rowClickTarget, out int matchId) && (_historyRequest is null))
            {
                _playerSession.MatchHistory.Select(matchId, _shownPageNumber);
                RequestedScreen = ScreenId.MatchDetail;
            }
        }

        private void PreviousPageButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_historyRequest is null)
            {
                StartHistoryRequest(_shownPageNumber - PageStep);
            }
        }

        private void NextPageButtonOnClick(object sender, MyraEventArgs e)
        {
            if (_historyRequest is null)
            {
                StartHistoryRequest(_shownPageNumber + PageStep);
            }
        }

        private void RetryButtonOnClick(object sender, MyraEventArgs e)
        {
            StartHistoryRequest(_requestedPageNumber);
        }

        private void BackButtonOnClick(object sender, MyraEventArgs e)
        {
            RequestedScreen = ScreenId.MainMenu;
        }
    }
}
