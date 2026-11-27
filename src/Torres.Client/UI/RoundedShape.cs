using Microsoft.Xna.Framework;

namespace Torres.Client.UI
{
    internal readonly record struct RoundedShape(int TextureSize, Vector2 Center, Vector2 HalfSize, float CornerRadius);
}
