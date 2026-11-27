namespace Torres.Client.Avatars
{
    internal interface IAvatarCache
    {
        void Put(string avatarReference, byte[] thumbnail);
    }
}
