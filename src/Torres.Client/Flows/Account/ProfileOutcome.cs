using Torres.Client.Avatars;

namespace Torres.Client.Flows
{
    internal sealed record ProfileOutcome(ProfileDetails? Details, ProfileMessage? Message, PreparedAvatar? Preview, bool IsSaved);
}
