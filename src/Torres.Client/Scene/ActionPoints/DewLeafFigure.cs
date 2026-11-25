using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class DewLeafFigure : IDisposable
    {
        private const int LeafDropSpots = 5;
        private const float FirstDropShare = 0.24f;
        private const float DropShareStep = 0.09f;
        private const float DropLift = 0.055f;
        private const float DropEmission = 0.35f;
        private const float LeafHeadingDegrees = -12f;
        private const float LeafScale = 0.85f;
        private const float TrembleHeight = 0.02f;
        private const double TremblePeriodSeconds = 0.18;
        private const float LabelDistance = 0.75f;
        private const double RollSeconds = 0.4;
        private const double FallSeconds = 0.35;
        private const double FallStaggerSeconds = 0.18;
        private const float TipShare = 0.97f;
        private const float FullScale = 1f;

        private static readonly Vector3 _dropScale = new Vector3(0.1f, 0.085f, 0.1f);
        private static readonly Vector3 _leafPosition = new Vector3(-3.6f, TableShapes.TableTop, 5.4f);

        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _leafMesh;
        private readonly StaticMesh _dropMesh;
        private readonly Matrix _leafWorld = Matrix.CreateScale(LeafScale) * BoardGeometry.CreateHeadingRotation(LeafHeadingDegrees) * Matrix.CreateTranslation(_leafPosition);

        private readonly List<FallingDrop> _fallingDrops = new List<FallingDrop>();

        private int _shownActionPoints;
        private int _previewCost;

        internal DewLeafFigure(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            var leafBuilder = new MeshBuilder();
            DewLeafShapes.AddLeaf(leafBuilder);
            _leafMesh = leafBuilder.BuildMesh(graphicsDevice);
            var dropBuilder = new MeshBuilder();
            dropBuilder.AddSphere(Matrix.Identity, Color.White);
            _dropMesh = dropBuilder.BuildMesh(graphicsDevice);
        }

        internal static Vector3 LabelAnchor { get; } = _leafPosition + new Vector3(0f, 0f, LabelDistance);

        internal void ShowActionPoints(int actionPoints, int previewCost, TimeSpan sceneTime)
        {
            for (var dropIndex = _shownActionPoints - 1; dropIndex >= actionPoints; dropIndex--)
            {
                TimeSpan staggerTime = TimeSpan.FromSeconds(FallStaggerSeconds * (_shownActionPoints - 1 - dropIndex));
                _fallingDrops.Add(new FallingDrop(dropIndex, sceneTime + staggerTime));
            }

            _shownActionPoints = actionPoints;
            _previewCost = previewCost;
        }

        internal void ForgetActionPoints()
        {
            _shownActionPoints = 0;
            _fallingDrops.Clear();
        }

        internal void Draw(BasicEffect sceneEffect, TimeSpan sceneTime)
        {
            RasterizerState frontRasterizerState = _graphicsDevice.RasterizerState;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            sceneEffect.World = _leafWorld;
            _leafMesh.Draw(_graphicsDevice, sceneEffect);
            _graphicsDevice.RasterizerState = frontRasterizerState;
            for (var dropIndex = 0; dropIndex < _shownActionPoints; dropIndex++)
            {
                DrawDrop(sceneEffect, sceneTime, dropIndex);
            }

            _fallingDrops.RemoveAll(fallingDrop => SceneTiming.FindProgress(sceneTime, fallingDrop.StartTime, RollSeconds + FallSeconds) >= FullScale);
            foreach (FallingDrop fallingDrop in _fallingDrops)
            {
                DrawFallingDrop(sceneEffect, sceneTime, fallingDrop);
            }

            sceneEffect.DiffuseColor = Vector3.One;
            sceneEffect.EmissiveColor = Vector3.Zero;
            sceneEffect.World = Matrix.Identity;
        }

        public void Dispose()
        {
            _leafMesh.Dispose();
            _dropMesh.Dispose();
        }
        private static float FindDropShare(int dropIndex)
        {
            float dropShare = FirstDropShare + (DropShareStep * dropIndex);

            return dropShare;
        }

        private void DrawFallingDrop(BasicEffect sceneEffect, TimeSpan sceneTime, FallingDrop fallingDrop)
        {
            float rollProgress = SceneTiming.FindProgress(sceneTime, fallingDrop.StartTime, RollSeconds);
            TimeSpan fallStart = fallingDrop.StartTime + TimeSpan.FromSeconds(RollSeconds);
            float fallProgress = SceneTiming.FindProgress(sceneTime, fallStart, FallSeconds);
            float ribShare = MathHelper.Lerp(FindDropShare(fallingDrop.DropIndex), TipShare, MathHelper.SmoothStep(0f, FullScale, rollProgress));
            Vector3 dropCenter = DewLeafShapes.FindRibPoint(ribShare) + new Vector3(0f, DropLift, 0f);
            dropCenter.Y = MathHelper.Lerp(dropCenter.Y, 0f, fallProgress * fallProgress);
            float dropScale = FullScale - fallProgress;
            Color dropColor = fallingDrop.DropIndex < LeafDropSpots ? GardenPalette.DewDrop : GardenPalette.ExtraDewDrop;
            sceneEffect.World = Matrix.CreateScale(_dropScale * dropScale) * Matrix.CreateTranslation(dropCenter) * _leafWorld;
            sceneEffect.DiffuseColor = dropColor.ToVector3();
            sceneEffect.EmissiveColor = dropColor.ToVector3() * DropEmission;
            _dropMesh.Draw(_graphicsDevice, sceneEffect);
        }

        private void DrawDrop(BasicEffect sceneEffect, TimeSpan sceneTime, int dropIndex)
        {
            Vector3 dropCenter = DewLeafShapes.FindRibPoint(FindDropShare(dropIndex)) + new Vector3(0f, DropLift, 0f);
            if (dropIndex >= _shownActionPoints - _previewCost)
            {
                dropCenter.Y += TrembleHeight * SceneTiming.FindSignedCycleWave(sceneTime, TremblePeriodSeconds);
            }

            Color dropColor = dropIndex < LeafDropSpots ? GardenPalette.DewDrop : GardenPalette.ExtraDewDrop;
            sceneEffect.World = Matrix.CreateScale(_dropScale) * Matrix.CreateTranslation(dropCenter) * _leafWorld;
            sceneEffect.DiffuseColor = dropColor.ToVector3();
            sceneEffect.EmissiveColor = dropColor.ToVector3() * DropEmission;
            _dropMesh.Draw(_graphicsDevice, sceneEffect);
        }

        private readonly record struct FallingDrop(int DropIndex, TimeSpan StartTime);
    }
}
