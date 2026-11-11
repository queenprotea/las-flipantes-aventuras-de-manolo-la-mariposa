using System;
using System.Collections.Generic;

namespace Torres.Client.Avatars
{
    internal sealed class AvatarViewSet
    {
        private readonly List<AvatarView> _avatarViews = new List<AvatarView>();
        private readonly AvatarCache _avatarCache;
        private readonly int _avatarSize;

        internal AvatarViewSet(AvatarCache avatarCache, int avatarSize)
        {
            ArgumentNullException.ThrowIfNull(avatarCache);

            _avatarCache = avatarCache;
            _avatarSize = avatarSize;
        }

        internal AvatarView Create(string? avatarReference)
        {
            var avatarView = new AvatarView(_avatarCache, _avatarSize) { AvatarReference = avatarReference };
            avatarView.Refresh();
            _avatarViews.Add(avatarView);

            return avatarView;
        }

        internal void Clear()
        {
            _avatarViews.Clear();
        }

        internal void Refresh()
        {
            foreach (AvatarView avatarView in _avatarViews)
            {
                avatarView.Refresh();
            }
        }
    }
}
