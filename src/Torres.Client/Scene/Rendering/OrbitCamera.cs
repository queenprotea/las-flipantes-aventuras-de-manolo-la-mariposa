using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal sealed class OrbitCamera
    {
        private const float FieldOfViewDegrees = 35f;
        private const float NearPlaneDistance = 0.1f;
        private const float FarPlaneDistance = 100f;
        private const float InitialYawDegrees = 0f;
        private const float InitialPitchDegrees = 42f;
        private const float InitialDistance = 15.5f;
        private const float MinimumPitchDegrees = 15f;
        private const float MaximumPitchDegrees = 85f;
        private const float MinimumDistance = 6f;
        private const float MaximumDistance = 22f;
        private const float YawDegreesPerPixel = 0.46f;
        private const float PitchDegreesPerPixel = 0.34f;
        private const float ZoomPerWheelUnit = 0.001f;
        private const float UnchangedDistanceFactor = 1f;
        private const float TargetHeight = 0.3f;

        private static readonly Vector3 _target = new Vector3(0f, TargetHeight, 0f);

        private float _yawDegrees = InitialYawDegrees;
        private float _pitchDegrees = InitialPitchDegrees;
        private float _distance = InitialDistance;

        internal void Rotate(Vector2 dragStepPixels)
        {
            _yawDegrees -= dragStepPixels.X * YawDegreesPerPixel;
            _pitchDegrees = MathHelper.Clamp(
                _pitchDegrees + (dragStepPixels.Y * PitchDegreesPerPixel),
                MinimumPitchDegrees,
                MaximumPitchDegrees);
        }

        internal void Zoom(float wheelDelta)
        {
            float zoomFactor = UnchangedDistanceFactor - (wheelDelta * ZoomPerWheelUnit);
            _distance = MathHelper.Clamp(_distance * zoomFactor, MinimumDistance, MaximumDistance);
        }

        internal Matrix CreateViewMatrix()
        {
            float yawRadians = MathHelper.ToRadians(_yawDegrees);
            float pitchRadians = MathHelper.ToRadians(_pitchDegrees);
            float horizontalReach = MathF.Cos(pitchRadians) * _distance;
            var cameraOffset = new Vector3(
                MathF.Sin(yawRadians) * horizontalReach,
                MathF.Sin(pitchRadians) * _distance,
                MathF.Cos(yawRadians) * horizontalReach);
            Matrix viewMatrix = Matrix.CreateLookAt(_target + cameraOffset, _target, Vector3.Up);

            return viewMatrix;
        }

        internal Matrix CreateProjectionMatrix(float aspectRatio)
        {
            float fieldOfViewRadians = MathHelper.ToRadians(FieldOfViewDegrees);
            Matrix projectionMatrix = Matrix.CreatePerspectiveFieldOfView(
                fieldOfViewRadians, aspectRatio, NearPlaneDistance, FarPlaneDistance);

            return projectionMatrix;
        }
    }
}
