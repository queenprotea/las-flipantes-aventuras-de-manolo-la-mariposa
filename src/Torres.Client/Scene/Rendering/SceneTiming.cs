using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class SceneTiming
    {
        private const float WaveMidpoint = 0.5f;
        private const float WaveHalfRange = 0.5f;
        private const float ArcShapeFactor = 4f;

        internal static float FindCycleWave(TimeSpan sceneTime, double periodSeconds)
        {
            float cycleWave = WaveMidpoint + (WaveHalfRange * FindSignedCycleWave(sceneTime, periodSeconds));

            return cycleWave;
        }

        internal static float FindSignedCycleWave(TimeSpan sceneTime, double periodSeconds)
        {
            float signedWave = MathF.Sin(FindCycleRadians(sceneTime, periodSeconds));

            return signedWave;
        }

        internal static float FindArcLift(float arcHeight, float travelShare)
        {
            float arcLift = arcHeight * ArcShapeFactor * travelShare * (1f - travelShare);

            return arcLift;
        }

        internal static float FindCycleRadians(TimeSpan sceneTime, double periodSeconds)
        {
            double cycleShare = (sceneTime.TotalSeconds % periodSeconds) / periodSeconds;
            float cycleRadians = (float)(cycleShare * MathHelper.TwoPi);

            return cycleRadians;
        }
    }
}
