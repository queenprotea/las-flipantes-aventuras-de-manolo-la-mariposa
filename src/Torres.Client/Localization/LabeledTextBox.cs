using Myra.Graphics2D.UI;

using Torres.Client.Ui;

namespace Torres.Client.Localization
{
    internal sealed class LabeledTextBox : VerticalStackPanel
    {
        private readonly TextBox _box = new TextBox();

        internal LabeledTextBox(string labelKey, bool isSecret)
        {
            Spacing = 4;
            _box.PasswordField = isSecret;
            _box.Width = Theme.CardWidth - (2 * Theme.Margin);
            Widgets.Add(new LocalizedLabel(labelKey) { TextColor = Theme.MutedText });
            Widgets.Add(_box);
        }

        internal string Value => _box.Text ?? string.Empty;

        internal TextBox Box => _box;
    }
}
