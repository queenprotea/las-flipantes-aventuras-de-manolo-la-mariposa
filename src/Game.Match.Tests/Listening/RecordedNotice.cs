using System.Collections.Generic;

namespace Game.Match.Tests
{
    internal sealed record RecordedNotice(RecordedNoticeKind Kind, IReadOnlyList<MatchRecipientView> RecipientViews);
}
