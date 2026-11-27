using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct LeafRing(int LeafCount, float PhaseDegrees, float Distance, float Lift, Vector3 Scale, float TiltDegrees, Color Color, bool IsCoarse);
}
