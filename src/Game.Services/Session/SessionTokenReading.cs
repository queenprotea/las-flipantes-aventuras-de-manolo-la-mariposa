namespace Game.Services
{
    internal sealed record SessionTokenReading(SessionTokenStatus Status, int PlayerId, string Username);
}
