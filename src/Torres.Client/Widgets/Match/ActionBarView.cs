using Game.Contracts;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.Scene;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal sealed class ActionBarView
    {
        private const float DisabledOpacity = 0.5f;

        private readonly FigureIcons _figureIcons;
        private readonly VolumeTextures _volumeTextures;
        private readonly Image _caterpillarIcon;
        private readonly ActionPill _caterpillarPill;
        private readonly ActionPill _movePill;
        private readonly ActionPill _buildPill;

        private TurnActionMode? _chosenMode;
        private ActionPill? _hoveredPill;

        internal ActionBarView(FigureIcons figureIcons, VolumeTextures volumeTextures)
        {
            _figureIcons = figureIcons;
            _volumeTextures = volumeTextures;
            _caterpillarIcon = new Image { Width = MatchLayout.ActionIconWidth, Height = MatchLayout.ActionIconHeight, VerticalAlignment = VerticalAlignment.Center };
            _caterpillarPill = CreatePill(TextKeys.Match.CaterpillarActionButton, _caterpillarIcon, TurnActionMode.NewCaterpillar);
            _movePill = CreatePill(TextKeys.Match.MoveActionButton, new Label { Text = DisplaySymbols.MoveArrow, Font = Fonts.Body, VerticalAlignment = VerticalAlignment.Center }, TurnActionMode.Move);
            _buildPill = CreatePill(TextKeys.Match.BuildActionButton, new Image { Width = MatchLayout.ActionIconHeight, Height = MatchLayout.ActionIconHeight, Renderable = figureIcons.SproutIcon, VerticalAlignment = VerticalAlignment.Center }, TurnActionMode.Build);
            var actionRow = new VerticalStackPanel
            {
                Spacing = MatchLayout.ActionPillSpacing,
                Width = MatchLayout.ActionColumnWidth,
            };
            actionRow.Widgets.Add(_caterpillarPill.Button);
            actionRow.Widgets.Add(_movePill.Button);
            actionRow.Widgets.Add(_buildPill.Button);
            Root = actionRow;
        }

        internal Widget Root { get; }

        internal int HoveredCost => (_hoveredPill is ActionPill hoveredPill) && hoveredPill.Button.Enabled ? hoveredPill.Cost : 0;

        internal void Show(MatchView matchView, SeatColor viewerColor, bool canAct, TurnActionMode chosenMode)
        {
            _caterpillarIcon.Renderable = _figureIcons.FindCaterpillarIcon(viewerColor);
            ShowPill(_caterpillarPill, matchView.ActionCosts.CaterpillarPlacement, canAct && (matchView.PlacementTargets.Count > 0), chosenMode);
            ShowPill(_movePill, matchView.ActionCosts.Movement, canAct && (matchView.MoveTargets.Count > 0), chosenMode);
            ShowPill(_buildPill, matchView.ActionCosts.FlowerLevelAddition, canAct && (matchView.BuildTargets.Count > 0), chosenMode);
        }

        internal TurnActionMode? TakeChosenMode()
        {
            TurnActionMode? chosenMode = _chosenMode;
            _chosenMode = null;

            return chosenMode;
        }

        private ActionPill CreatePill(string textKey, Widget actionIcon, TurnActionMode actionMode)
        {
            var iconCircle = new Panel
            {
                Width = MatchLayout.ActionIconCircleSize,
                Height = MatchLayout.ActionIconCircleSize,
                Background = _volumeTextures.CircleBrush,
                VerticalAlignment = VerticalAlignment.Center,
            };
            actionIcon.HorizontalAlignment = HorizontalAlignment.Center;
            iconCircle.Widgets.Add(actionIcon);
            var nameLabel = new LocalizedLabel(textKey) { Font = Fonts.Body };
            var costLabel = new Label { Font = Fonts.Label };
            var unitLabel = new LocalizedLabel(TextKeys.Match.ActionPointsLabel) { Font = Fonts.Label };
            var costRow = new HorizontalStackPanel { Spacing = MatchLayout.TableLabelSpacing };
            costRow.Widgets.Add(costLabel);
            costRow.Widgets.Add(unitLabel);
            var pillText = new VerticalStackPanel { VerticalAlignment = VerticalAlignment.Center };
            pillText.Widgets.Add(nameLabel);
            pillText.Widgets.Add(costRow);
            var pillContent = new HorizontalStackPanel { Spacing = Metrics.FieldLabelSpacing, VerticalAlignment = VerticalAlignment.Center };
            pillContent.Widgets.Add(iconCircle);
            pillContent.Widgets.Add(pillText);
            var pillButton = new Button
            {
                Content = pillContent,
                Padding = MatchLayout.ActionPillPadding,
                Background = _volumeTextures.PillBrush,
                OverBackground = _volumeTextures.HoveredPillBrush,
                PressedBackground = _volumeTextures.ChosenPillBrush,
                DisabledBackground = _volumeTextures.PillBrush,
                HorizontalAlignment = HorizontalAlignment.Stretch,
            };
            var actionPill = new ActionPill(pillButton, nameLabel, costLabel, unitLabel, actionIcon, actionMode);
            pillButton.Click += (_, _) => _chosenMode = actionMode;
            pillButton.MouseEntered += (_, _) => _hoveredPill = actionPill;
            pillButton.MouseLeft += (_, _) => _hoveredPill = _hoveredPill == actionPill ? null : _hoveredPill;

            return actionPill;
        }

        private void ShowPill(ActionPill actionPill, int actionCost, bool isEnabled, TurnActionMode chosenMode)
        {
            bool isChosen = actionPill.Mode == chosenMode;
            Color textColor = isChosen ? Theme.Surface : Theme.Ink;
            actionPill.Cost = actionCost;
            actionPill.Button.Enabled = isEnabled;
            actionPill.Button.Opacity = isEnabled ? 1f : DisabledOpacity;
            actionPill.Button.Background = isChosen ? _volumeTextures.ChosenPillBrush : _volumeTextures.PillBrush;
            actionPill.Button.OverBackground = isChosen ? _volumeTextures.ChosenPillBrush : _volumeTextures.HoveredPillBrush;
            actionPill.NameLabel.TextColor = textColor;
            actionPill.CostLabel.TextColor = isChosen ? Theme.Surface : Theme.MutedInk;
            actionPill.UnitLabel.TextColor = actionPill.CostLabel.TextColor;
            actionPill.CostLabel.Text = DisplayFormats.FormatInteger(actionCost);
            if (actionPill.Icon is Label iconLabel)
            {
                iconLabel.TextColor = Theme.ChosenAction;
            }
        }

        private sealed class ActionPill
        {
            internal ActionPill(Button button, LocalizedLabel nameLabel, Label costLabel, LocalizedLabel unitLabel, Widget icon, TurnActionMode mode)
            {
                Button = button;
                NameLabel = nameLabel;
                CostLabel = costLabel;
                UnitLabel = unitLabel;
                Icon = icon;
                Mode = mode;
            }

            internal Button Button { get; }

            internal LocalizedLabel NameLabel { get; }

            internal Label CostLabel { get; }

            internal LocalizedLabel UnitLabel { get; }

            internal Widget Icon { get; }

            internal TurnActionMode Mode { get; }

            internal int Cost { get; set; }
        }
    }
}
