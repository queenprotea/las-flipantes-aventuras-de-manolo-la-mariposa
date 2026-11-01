namespace Game.Services
{
    internal sealed record SessionTokenReading(SessionTokenStatus Status, int OwnerId, string DisplayName, bool IsGuest);
}
