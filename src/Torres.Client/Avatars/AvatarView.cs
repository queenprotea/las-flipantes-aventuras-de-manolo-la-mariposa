using System;

using Myra.Graphics2D;
using Myra.Graphics2D.UI;

using Torres.Client.Ui;

namespace Torres.Client.Avatars
{
    internal sealed class AvatarView
    {
        private readonly AvatarCache _avatarCache;
        private readonly Image _image;

        internal AvatarView(AvatarCache avatarCache, int avatarSize)
        {
            ArgumentNullException.ThrowIfNull(avatarCache);

            _avatarCache = avatarCache;
            _image = new Image
            {
                Width = avatarSize,
                Height = avatarSize,
                Renderable = avatarCache.DefaultImage,
            };
            Root = new Panel
            {
                Width = avatarSize,
                Height = avatarSize,
                Border = Theme.LineBrush,
                BorderThickness = Sizes.Border,
                VerticalAlignment = VerticalAlignment.Center,
            };
            Root.Widgets.Add(_image);
        }

        internal Panel Root { get; }

        internal string? AvatarReference { get; set; }

        internal IImage? PreviewImage { get; set; }

        internal void Refresh()
        {
            _image.Renderable = PreviewImage ?? _avatarCache.FindImage(AvatarReference);
        }
    }
}
