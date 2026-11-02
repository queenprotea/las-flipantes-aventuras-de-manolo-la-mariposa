using System.Text;

namespace Game.Services
{
    internal static class ChatTextNormalizer
    {
        private const char UndrawableReplacement = '?';
        private const char ControlReplacement = ' ';

        internal static string Normalize(string? text)
        {
            var normalizedBuilder = new StringBuilder();
            foreach (Rune character in (text ?? string.Empty).EnumerateRunes())
            {
                normalizedBuilder.Append(ToDrawable(character));
            }

            string normalizedText = normalizedBuilder.ToString().Trim();

            return normalizedText;
        }

        private static char ToDrawable(Rune character)
        {
            char drawableCharacter;
            if (!character.IsBmp || (character == Rune.ReplacementChar))
            {
                drawableCharacter = UndrawableReplacement;
            }
            else if (Rune.IsControl(character))
            {
                drawableCharacter = ControlReplacement;
            }
            else
            {
                drawableCharacter = (char)character.Value;
            }

            return drawableCharacter;
        }
    }
}
