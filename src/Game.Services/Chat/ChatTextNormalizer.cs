using System.Text;

namespace Game.Services
{
    internal static class ChatTextNormalizer
    {
        private const char UndrawableReplacement = '?';
        private const char ControlCharacterReplacement = ' ';

        internal static string Normalize(string? messageText)
        {
            var drawableTextBuilder = new StringBuilder();
            foreach (Rune character in (messageText ?? string.Empty).EnumerateRunes())
            {
                drawableTextBuilder.Append(ReplaceIfUndrawable(character));
            }

            string normalizedText = drawableTextBuilder.ToString().Trim();

            return normalizedText;
        }

        private static char ReplaceIfUndrawable(Rune character)
        {
            char drawableCharacter;
            if (!character.IsBmp || (character == Rune.ReplacementChar))
            {
                drawableCharacter = UndrawableReplacement;
            }
            else if (Rune.IsControl(character))
            {
                drawableCharacter = ControlCharacterReplacement;
            }
            else
            {
                drawableCharacter = (char)character.Value;
            }

            return drawableCharacter;
        }
    }
}
