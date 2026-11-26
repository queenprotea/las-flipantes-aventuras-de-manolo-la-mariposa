namespace Torres.Client.Threading
{
    internal sealed record FinishedRequest<TReply>(bool IsSuccessful, TReply? Reply);
}
