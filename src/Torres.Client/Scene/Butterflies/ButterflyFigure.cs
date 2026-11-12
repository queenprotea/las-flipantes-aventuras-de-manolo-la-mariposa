using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class ButterflyFigure : IDisposable
    {
        private const float HoverHeight = 0.62f;
        private const float BobHeight = 0.035f;
        private const double BobPeriodSeconds = 2.4;
        private const float FlapDegrees = 38f;
        private const float RestingWingDegrees = 12f;
        private const double FlapPeriodSeconds = 0.55;
        private const double FlightFlapPeriodSeconds = 0.24;
        private const float RestingHeadingDegrees = 300f;
        private const double FlightSeconds = 1.1;
        private const float FlightArcHeight = 0.5f;
        private const float FlightArcHeightPerCell = 0.12f;
        private const float AppearanceDropHeight = 1.6f;
        private const float MinimumHeadingTravel = 0.01f;
        private const float ShadowLift = 0.006f;
        private const float RestingPointTolerance = 0.001f;
        private const float LeftSideSign = -1f;
        private const float RightSideSign = 1f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _bodyMesh;
        private readonly StaticMesh _leftWingsMesh;
        private readonly StaticMesh _rightWingsMesh;
        private readonly StaticMesh _shadowMesh;

        private Vector3? _restingPoint;
        private Vector3 _flightOrigin;
        private TimeSpan _flightStart;
        private float _headingDegrees = RestingHeadingDegrees;

        internal ButterflyFigure(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            var bodyBuilder = new MeshBuilder();
            ButterflyShapes.AddBody(bodyBuilder);
            _bodyMesh = bodyBuilder.BuildMesh(graphicsDevice);

            var leftWingsBuilder = new MeshBuilder();
            ButterflyShapes.AddWings(leftWingsBuilder, LeftSideSign);
            _leftWingsMesh = leftWingsBuilder.BuildMesh(graphicsDevice);

            var rightWingsBuilder = new MeshBuilder();
            ButterflyShapes.AddWings(rightWingsBuilder, RightSideSign);
            _rightWingsMesh = rightWingsBuilder.BuildMesh(graphicsDevice);

            var shadowBuilder = new MeshBuilder();
            ButterflyShapes.AddShadow(shadowBuilder);
            _shadowMesh = shadowBuilder.BuildMesh(graphicsDevice);
        }

        internal bool IsShown => _restingPoint.HasValue;

        internal void FlyTo(Vector3 restingPoint, TimeSpan sceneTime)
        {
            bool isSameRestingPoint = (_restingPoint is Vector3 shownRestingPoint) && (Vector3.Distance(shownRestingPoint, restingPoint) < RestingPointTolerance);
            if (isSameRestingPoint)
            {
                return;
            }

            _flightOrigin = _restingPoint.HasValue ? FindFlightPoint(sceneTime) : restingPoint + new Vector3(0f, AppearanceDropHeight, 0f);
            Vector3 travel = restingPoint - _flightOrigin;
            var horizontalTravel = new Vector3(travel.X, 0f, travel.Z);
            if (horizontalTravel.Length() > MinimumHeadingTravel)
            {
                _headingDegrees = BoardGeometry.GetHeadingDegrees(horizontalTravel);
            }

            _restingPoint = restingPoint;
            _flightStart = sceneTime;
        }

        internal void LandAt(Vector3 restingPoint)
        {
            _restingPoint = restingPoint;
            _flightOrigin = restingPoint;
            _flightStart = TimeSpan.Zero;
        }

        internal void Hide()
        {
            _restingPoint = null;
        }

        internal Vector3 FindFlightPoint(TimeSpan sceneTime)
        {
            Vector3 flightPoint = _flightOrigin;
            if (_restingPoint is Vector3 shownRestingPoint)
            {
                float easedShare = MathHelper.SmoothStep(0f, 1f, FindFlightShare(sceneTime));
                float arcHeight = FlightArcHeight + (FlightArcHeightPerCell * Vector3.Distance(_flightOrigin, shownRestingPoint));
                float arcLift = SceneTiming.FindArcLift(arcHeight, easedShare);
                flightPoint = Vector3.Lerp(_flightOrigin, shownRestingPoint, easedShare) + new Vector3(0f, arcLift, 0f);
            }

            return flightPoint;
        }

        internal void Draw(BasicEffect sceneEffect, TimeSpan sceneTime)
        {
            bool isFlying = FindFlightShare(sceneTime) < 1f;
            double flapPeriodSeconds = isFlying ? FlightFlapPeriodSeconds : FlapPeriodSeconds;
            Matrix bodyWorld = BoardGeometry.CreateHeadingRotation(_headingDegrees) * Matrix.CreateTranslation(FindHoverPoint(sceneTime));
            float wingLiftRadians = MathHelper.ToRadians(RestingWingDegrees + (FlapDegrees * SceneTiming.FindCycleWave(sceneTime, flapPeriodSeconds)));

            sceneEffect.World = bodyWorld;
            _bodyMesh.Draw(_graphicsDevice, sceneEffect);
            sceneEffect.World = Matrix.CreateRotationX(wingLiftRadians) * bodyWorld;
            _leftWingsMesh.Draw(_graphicsDevice, sceneEffect);
            sceneEffect.World = Matrix.CreateRotationX(-wingLiftRadians) * bodyWorld;
            _rightWingsMesh.Draw(_graphicsDevice, sceneEffect);
        }

        internal void DrawShadow(BasicEffect sceneEffect, TimeSpan sceneTime, float groundHeight)
        {
            Vector3 flightPoint = FindFlightPoint(sceneTime);
            float heightAboveGround = flightPoint.Y + HoverHeight - groundHeight;
            var groundPoint = new Vector3(flightPoint.X, groundHeight + ShadowLift, flightPoint.Z);
            Vector3 shadowCenter = groundPoint + SceneLighting.GetShadowOffset(heightAboveGround);
            sceneEffect.World = BoardGeometry.CreateHeadingRotation(_headingDegrees) * Matrix.CreateTranslation(shadowCenter);
            _shadowMesh.Draw(_graphicsDevice, sceneEffect);
        }

        public void Dispose()
        {
            _bodyMesh.Dispose();
            _leftWingsMesh.Dispose();
            _rightWingsMesh.Dispose();
            _shadowMesh.Dispose();
        }

        private float FindFlightShare(TimeSpan sceneTime)
        {
            double flightSeconds = (sceneTime - _flightStart).TotalSeconds;
            float flightShare = (float)Math.Clamp(flightSeconds / FlightSeconds, 0d, 1d);

            return flightShare;
        }

        private Vector3 FindHoverPoint(TimeSpan sceneTime)
        {
            float bobOffset = BobHeight * SceneTiming.FindSignedCycleWave(sceneTime, BobPeriodSeconds);
            Vector3 hoverPoint = FindFlightPoint(sceneTime) + new Vector3(0f, HoverHeight + bobOffset, 0f);

            return hoverPoint;
        }
    }
}
