using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal readonly record struct PartPose(Vector3 Center, Vector3 Scale, float TiltDegrees, float HeadingDegrees)
    {
        internal static PartPose CreateUpright(Vector3 center, Vector3 scale)
        {
            var uprightPose = new PartPose(center, scale, 0f, 0f);

            return uprightPose;
        }
    }
}
