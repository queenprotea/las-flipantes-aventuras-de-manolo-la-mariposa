using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct CaterpillarPose(Vector3 BodyOrigin, float HeadingDegrees, Color BodyColor, float WavePhaseRadians, float Scale);
}
