using System.Collections.Generic;

using Torres.Client.Avatars;

namespace Torres.Client.Tests
{
    internal sealed class RecordingAvatarCache : IAvatarCache
    {
        internal List<string> StoredReferences { get; } = new List<string>();

        public void Put(string avatarReference, byte[] thumbnail)
        {
            StoredReferences.Add(avatarReference);
        }
    }
}
