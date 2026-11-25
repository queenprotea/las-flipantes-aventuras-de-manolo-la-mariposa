using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct PetalRing(int PetalCount, float Radius, Vector3 PetalScale, float TiltDegrees, float PhaseDegrees, Color Color);
}
