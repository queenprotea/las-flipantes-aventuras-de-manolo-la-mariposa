using System;
using System.ComponentModel;
using System.Text;

using Microsoft.Xna.Framework.Input;

using Myra.Graphics2D.UI;

using TextCopy;

namespace Torres.Client.Widgets
{
    /// <summary>
    /// Single-line text box that keeps its text within a length limit, also when the player pastes.
    /// </summary>
    /// <remarks>
    /// Myra's own paste moves the cursor by the whole pasted length even when a handler of
    /// <see cref="TextBox.ValueChanging"/> rejects the text, which leaves the cursor past the end and crashes the next draw.
    /// This box never rejects a change: it cuts what does not fit before inserting it, and it turns off undo and redo,
    /// whose records go stale once the text is set by code.
    /// </remarks>
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
            bool isFull = (LongestText > 0) && !char.IsControl(c) && !HasSelection() && (CurrentText().Length >= LongestText);
            if (isFull)
            {
                OnLimitReached();
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
            string? clipboardText;
            try
            {
                clipboardText = Clipboard.GetText();
            }
            catch (Win32Exception)
            {
                clipboardText = null;
            }
            catch (NotSupportedException)
            {
                clipboardText = null;
            }

            return clipboardText ?? string.Empty;
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

            return singleLineBuilder.ToString();
        }

        private static string CutToLength(string text, int longestLength)
        {
            int keptLength = Math.Min(text.Length, longestLength);
            if ((keptLength > 0) && (keptLength < text.Length) && char.IsHighSurrogate(text[keptLength - 1]))
            {
                keptLength--;
            }

            return text.Substring(0, keptLength);
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
                OnLimitReached();
            }
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

        private void OnLimitReached()
        {
            LimitReached?.Invoke(this, EventArgs.Empty);
        }
    }
}
