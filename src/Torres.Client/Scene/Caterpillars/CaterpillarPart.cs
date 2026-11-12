using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct CaterpillarPart(Matrix Transform, Color Color, CaterpillarPartShape Shape);
}
