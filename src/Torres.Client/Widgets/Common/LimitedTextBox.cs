using System;
using System.ComponentModel;
using System.Text;

using Microsoft.Xna.Framework.Input;

using Myra.Graphics2D.UI;

using TextCopy;

namespace Torres.Client.Widgets
{
    /// <summary>
    /// Keeps a single-line text within its length limit when the player types or pastes, and turns off undo and redo.
    /// </summary>
    internal sealed class LimitedTextBox : TextBox
    {
        private const char LineFeed = '\n';
        private const char CarriageReturn = '\r';
        private const char Tab = '\t';
        private const char Space = ' ';

        internal LimitedTextBox(int longestText)
        {
            LongestText = longestText;
        }

        internal event EventHandler? LimitReached;

        internal int LongestText { get; set; }

        public override void OnKeyDown(Keys k)
        {
            bool isControlDown = IsControlDown();
            if (isControlDown && (k == Keys.V))
            {
                PasteWithinLimit();
            }
            else if (!isControlDown || ((k != Keys.Z) && (k != Keys.Y)))
            {
                base.OnKeyDown(k);
            }
        }

        public override void OnChar(char c)
        {
            bool isFull = !char.IsControl(c) && !HasSelection() && IsAtLimit();
            if (isFull)
            {
                OnLimitReached(EventArgs.Empty);
            }
            else
            {
                base.OnChar(c);
            }
        }

        private static bool IsControlDown()
        {
            KeyboardState keyboardState = Keyboard.GetState();
            bool isControlDown = keyboardState.IsKeyDown(Keys.LeftControl) || keyboardState.IsKeyDown(Keys.RightControl);

            return isControlDown;
        }

        private static string ReadClipboard()
        {
            string clipboardText;
            try
            {
                clipboardText = Clipboard.GetText() ?? string.Empty;
            }
            catch (Win32Exception)
            {
                clipboardText = string.Empty;
            }
            catch (NotSupportedException)
            {
                clipboardText = string.Empty;
            }

            return clipboardText;
        }

        private static string ToSingleLine(string pastedText)
        {
            var singleLineBuilder = new StringBuilder(pastedText.Length);
            foreach (char pastedCharacter in pastedText)
            {
                if ((pastedCharacter == LineFeed) || (pastedCharacter == Tab))
                {
                    singleLineBuilder.Append(Space);
                }
                else if (pastedCharacter != CarriageReturn)
                {
                    singleLineBuilder.Append(pastedCharacter);
                }
            }

            string singleLineText = singleLineBuilder.ToString();

            return singleLineText;
        }

        private static string CutToLength(string text, int longestLength)
        {
            int keptLength = Math.Min(text.Length, longestLength);
            if ((keptLength > 0) && (keptLength < text.Length) && char.IsHighSurrogate(text[keptLength - 1]))
            {
                keptLength--;
            }

            string fittingText = text.Substring(0, keptLength);

            return fittingText;
        }

        private void PasteWithinLimit()
        {
            if (!Enabled || Readonly)
            {
                return;
            }

            string currentText = CurrentText();
            int replacedStart = Math.Clamp(Math.Min(SelectStart, SelectEnd), 0, currentText.Length);
            int replacedEnd = Math.Clamp(Math.Max(SelectStart, SelectEnd), 0, currentText.Length);
            if (replacedStart == replacedEnd)
            {
                replacedStart = Math.Clamp(CursorPosition, 0, currentText.Length);
                replacedEnd = replacedStart;
            }

            string pastedText = ToSingleLine(ReadClipboard());
            int keptLength = currentText.Length - (replacedEnd - replacedStart);
            int freeLength = LongestText > 0 ? Math.Max(0, LongestText - keptLength) : pastedText.Length;
            string fittingText = CutToLength(pastedText, freeLength);
            if (fittingText.Length > 0)
            {
                Text = currentText.Substring(0, replacedStart) + fittingText + currentText.Substring(replacedEnd);
                CursorPosition = Math.Min(replacedStart + fittingText.Length, CurrentText().Length);
            }

            if (fittingText.Length < pastedText.Length)
            {
                OnLimitReached(EventArgs.Empty);
            }
        }

        private bool IsAtLimit()
        {
            bool isAtLimit = (LongestText > 0) && (CurrentText().Length >= LongestText);

            return isAtLimit;
        }

        private bool HasSelection()
        {
            bool hasSelection = SelectStart != SelectEnd;

            return hasSelection;
        }

        private string CurrentText()
        {
            string currentText = Text ?? string.Empty;

            return currentText;
        }

        private void OnLimitReached(EventArgs e)
        {
            LimitReached?.Invoke(this, e);
        }
    }
}
