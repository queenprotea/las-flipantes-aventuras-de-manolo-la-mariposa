namespace Game.Services
{
    internal sealed record ProfileChange(string Username, string? PreviousReference, string? NewReference);
}
