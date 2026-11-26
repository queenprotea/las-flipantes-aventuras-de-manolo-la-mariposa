using FontStashSharp.RichText;

using Microsoft.Xna.Framework;

using Myra.Graphics2D.UI;

using Torres.Client.Localization;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal static class LabelFactory
    {
        internal static LocalizedLabel CreateTitle(string textKey)
        {
            var title = new LocalizedLabel(textKey)
            {
                Font = Fonts.Title,
                TextColor = Theme.Ink,
                Wrap = true,
            };

            return title;
        }

        internal static LocalizedLabel CreateHint(string textKey)
        {
            var hint = new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MutedInk,
                Wrap = true,
            };

            return hint;
        }

        internal static LocalizedLabel CreateParagraph(string textKey)
        {
            var paragraph = new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.SoftInk,
                Wrap = true,
            };

            return paragraph;
        }

        internal static LocalizedLabel CreateErrorMessage(string textKey)
        {
            var errorMessage = new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.BlushInk,
                Wrap = true,
                Visible = false,
            };

            return errorMessage;
        }

        internal static LocalizedLabel CreateSuccessMessage(string textKey)
        {
            var successMessage = new LocalizedLabel(textKey)
            {
                Font = Fonts.Small,
                TextColor = Theme.MintInk,
                Wrap = true,
            };

            return successMessage;
        }

        internal static Label CreatePlayerNameLabel(string playerName, Color textColor)
        {
            var playerNameLabel = new Label
            {
                Text = playerName,
                Font = Fonts.Control,
                TextColor = textColor,
                AutoEllipsisMethod = AutoEllipsisMethod.Character,
                VerticalAlignment = VerticalAlignment.Center,
                SupportsCommands = false,
            };

            return playerNameLabel;
        }

        internal static Label CreateUsernameLabel(string username)
        {
            var usernameLabel = new Label
            {
                Text = username,
                Font = Fonts.Body,
                TextColor = Theme.Ink,
                VerticalAlignment = VerticalAlignment.Center,
                SupportsCommands = false,
            };

            return usernameLabel;
        }

        internal static LocalizedLabel CreateTag(string textKey, Color textColor)
        {
            var tagLabel = new LocalizedLabel(textKey)
            {
                Font = Fonts.Label,
                TextColor = textColor,
            };

            return tagLabel;
        }
    }
}
