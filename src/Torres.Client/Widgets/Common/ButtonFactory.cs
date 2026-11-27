using Microsoft.Xna.Framework;

using Myra.Graphics2D;

using Torres.Client.Localization;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal static class ButtonFactory
    {
        internal static LocalizedButton CreatePrimaryButton(string textKey)
        {
            LocalizedButton primaryButton = ButtonFactory.CreateBaseButton(textKey, Theme.MintInk);
            primaryButton.Background = Theme.MintBrush;
            primaryButton.OverBackground = Theme.MintAltBrush;
            primaryButton.PressedBackground = Theme.MintAltBrush;
            primaryButton.Border = Theme.MintLineBrush;

            return primaryButton;
        }

        internal static LocalizedButton CreateSecondaryButton(string textKey)
        {
            LocalizedButton secondaryButton = ButtonFactory.CreateBaseButton(textKey, Theme.Ink);
            secondaryButton.Background = Theme.SurfaceBrush;
            secondaryButton.OverBackground = Theme.SurfaceSunkenBrush;
            secondaryButton.PressedBackground = Theme.SurfaceSunkenBrush;
            secondaryButton.Border = Theme.StrongLineBrush;

            return secondaryButton;
        }

        internal static LocalizedButton CreateSmallSecondaryButton(string textKey)
        {
            LocalizedButton smallSecondaryButton = ButtonFactory.CreateSecondaryButton(textKey);
            smallSecondaryButton.LabelFont = Fonts.Small;
            smallSecondaryButton.Padding = Metrics.SmallButtonPadding;

            return smallSecondaryButton;
        }

        internal static LocalizedButton CreateDestructiveButton(string textKey)
        {
            LocalizedButton destructiveButton = ButtonFactory.CreateBaseButton(textKey, Theme.BlushInk);
            destructiveButton.Background = Theme.BlushTintBrush;
            destructiveButton.OverBackground = Theme.BlushTintBrush;
            destructiveButton.PressedBackground = Theme.BlushTintBrush;
            destructiveButton.Border = Theme.BlushBrush;

            return destructiveButton;
        }

        internal static LocalizedButton CreateLavenderButton(string textKey)
        {
            LocalizedButton lavenderButton = ButtonFactory.CreateBaseButton(textKey, Theme.LavenderInk);
            lavenderButton.Background = Theme.LavenderBrush;
            lavenderButton.OverBackground = Theme.LavenderTintBrush;
            lavenderButton.PressedBackground = Theme.LavenderTintBrush;
            lavenderButton.Border = Theme.LavenderLineBrush;

            return lavenderButton;
        }

        internal static LocalizedButton CreateLinkButton(string textKey)
        {
            var linkButton = new LocalizedButton(textKey)
            {
                LabelColor = Theme.LavenderInk,
                LabelFont = Fonts.Small,
                Padding = Metrics.LinkButtonPadding,
                Background = null,
                OverBackground = null,
                PressedBackground = null,
                Border = null,
                BorderThickness = new Thickness(0),
            };

            return linkButton;
        }

        private static LocalizedButton CreateBaseButton(string textKey, Color labelColor)
        {
            var baseButton = new LocalizedButton(textKey)
            {
                LabelColor = labelColor,
                LabelFont = Fonts.Control,
                Padding = Metrics.ButtonPadding,
                BorderThickness = Sizes.Border,
            };

            return baseButton;
        }
    }
}
