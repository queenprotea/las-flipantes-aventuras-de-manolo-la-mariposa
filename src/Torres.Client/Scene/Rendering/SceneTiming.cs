using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class SceneTiming
    {
        private const float WaveMidpoint = 0.5f;
        private const float WaveHalfRange = 0.5f;
        private const float ArcShapeFactor = 4f;
        private const float OvershootFactor = 1.70158f;
        private const float FullProgress = 1f;

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

        internal static float FindArcLift(float arcHeight, float travelProgress)
        {
            float arcLift = arcHeight * ArcShapeFactor * travelProgress * (1f - travelProgress);

            return arcLift;
        }

        internal static float FindProgress(TimeSpan sceneTime, TimeSpan startTime, double durationSeconds)
        {
            double elapsedSeconds = (sceneTime - startTime).TotalSeconds;
            float progress = (float)Math.Clamp(elapsedSeconds / durationSeconds, 0d, 1d);

            return progress;
        }

        internal static float EaseOutWithOvershoot(float progress)
        {
            float remaining = progress - FullProgress;
            float easedProgress = FullProgress + ((OvershootFactor + FullProgress) * remaining * remaining * remaining) + (OvershootFactor * remaining * remaining);

            return easedProgress;
        }

        internal static float FindCycleRadians(TimeSpan sceneTime, double periodSeconds)
        {
            double cycleProgress = (sceneTime.TotalSeconds % periodSeconds) / periodSeconds;
            float cycleRadians = (float)(cycleProgress * MathHelper.TwoPi);

            return cycleRadians;
        }
    }
}
