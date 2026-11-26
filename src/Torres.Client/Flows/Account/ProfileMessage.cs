namespace Torres.Client.Flows
{
    internal sealed record ProfileMessage(ProfileMessageTarget Target, string TextKey, long? FileBytes);
}
