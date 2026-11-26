using System;

using Torres.Client.Localization;

namespace Torres.Client.Flows
{
    internal sealed record Notice(string? TitleKey, string? BodyKey, string[]? BodyArguments, bool IsBan, DateTime? BanEndsAt)
    {
        internal static Notice CreateTitle(string titleKey)
        {
            var titleNotice = new Notice(titleKey, null, null, false, null);

            return titleNotice;
        }

        internal static Notice CreateMessage(string titleKey, string bodyKey, string[]? bodyArguments)
        {
            var messageNotice = new Notice(titleKey, bodyKey, bodyArguments, false, null);

            return messageNotice;
        }

        internal static Notice CreateServerError()
        {
            Notice serverErrorNotice = CreateMessage(TextKeys.Common.ServerErrorTitle, TextKeys.Common.ServerErrorDetail, null);

            return serverErrorNotice;
        }

        internal static Notice CreateBan(DateTime? banEndsAt)
        {
            var banNotice = new Notice(null, null, null, true, banEndsAt);

            return banNotice;
        }
    }
}
