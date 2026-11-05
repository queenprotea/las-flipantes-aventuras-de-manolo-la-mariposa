using System;
using System.Globalization;

using Torres.Client.Localization;

namespace Torres.Client.Ui
{
    internal static class DisplayFormats
    {
        private const string ThousandsSeparatedPattern = "N0";
        private const string ClockPattern = "{0}:{1:00}";
        private const int SecondsPerMinute = 60;
        private const int ExpiredClockSeconds = 0;

        internal static string FormatThousands(int number)
        {
            string formattedValue = number.ToString(ThousandsSeparatedPattern, CultureInfo.CurrentCulture);

            return formattedValue;
        }

        internal static string FormatRankPosition(int rank)
        {
            string rankPosition = LocalizedText.Format(TextKeys.Common.RankPosition, FormatInteger(rank));

            return rankPosition;
        }

        internal static string FormatBonus(int points)
        {
            string bonusText = DisplaySymbols.BonusSign + points.ToString(CultureInfo.CurrentCulture);

            return bonusText;
        }

        internal static string FormatChatAuthor(string displayName)
        {
            string authorText = displayName + DisplaySymbols.ChatAuthorSeparator;

            return authorText;
        }

        internal static string FormatInteger(int number)
        {
            string formattedValue = number.ToString(CultureInfo.CurrentCulture);

            return formattedValue;
        }

        internal static string FormatRemainingTime(TimeSpan remainingTime)
        {
            TimeSpan shownTime = remainingTime < TimeSpan.Zero ? TimeSpan.Zero : remainingTime;
            string remainingText = LocalizedText.Format(TextKeys.Common.RemainingTime, FormatInteger((int)shownTime.TotalHours), FormatInteger(shownTime.Minutes));

            return remainingText;
        }

        internal static string FormatCountdown(TimeSpan remainingTime)
        {
            int remainingSeconds = Math.Max(ExpiredClockSeconds, (int)Math.Ceiling(remainingTime.TotalSeconds));
            string clockText = string.Format(CultureInfo.CurrentCulture, ClockPattern, remainingSeconds / SecondsPerMinute, remainingSeconds % SecondsPerMinute);

            return clockText;
        }

        internal static string FormatDate(DateTime dateValue, string datePattern)
        {
            string formattedDate = dateValue.ToString(datePattern, CultureInfo.CurrentCulture);

            return formattedDate;
        }
    }
}
