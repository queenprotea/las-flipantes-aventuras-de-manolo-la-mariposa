using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class SproutFigure : IDisposable
    {
        private const float PointsLeafHeight = 2.45f;
        private const float PointsLeafScale = 1f;
        private const float PointsLeafDroopDegrees = -5f;
        private const float CaterpillarLeafHeight = 0.75f;
        private const float CaterpillarLeafScale = 1.38f;
        private const float CaterpillarLeafDroopDegrees = -4f;
        private const int CaterpillarLeafSeed = 11;
        private const float CaterpillarScale = 0.62f;
        private const float CaterpillarHeadingDegrees = 0f;
        private const float CaterpillarLift = 0.01f;
        private const float CaterpillarBackShift = 0.12f;
        private const float FirstBudAngleDegrees = 60f;
        private const float LastBudAngleDegrees = -20f;
        private const float MiddleBudFraction = 0.5f;
        private const int SingleBudCount = 1;

        private static readonly Vector2[] _caterpillarSpots = new Vector2[]
        {
            new Vector2(0.3f, -0.45f), new Vector2(0.5f, -0.45f), new Vector2(0.7f, -0.45f), new Vector2(0.4f, 0.45f), new Vector2(0.6f, 0.45f),
        };

        private static readonly Vector3 _pointsLabelOffset = new Vector3(3.1f, 0.4f, 0f);
        private static readonly Vector3 _caterpillarsLabelOffset = new Vector3(2.3f, -0.9f, 0f);
        private static readonly Vector3 _buildsLabelOffset = new Vector3(1.75f, 0.05f, 0f);

        private static readonly Matrix _pointsLeafWorld = CreateLeafWorld(PointsLeafHeight, PointsLeafScale, PointsLeafDroopDegrees);
        private static readonly Matrix _caterpillarLeafWorld = CreateLeafWorld(CaterpillarLeafHeight, CaterpillarLeafScale, CaterpillarLeafDroopDegrees);

        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _stemMesh;
        private readonly StaticMesh _caterpillarLeafMesh;
        private readonly StaticMesh _budMesh;
        private readonly DewLeafFigure _dewLeafFigure;
        private readonly CaterpillarFigure _caterpillarFigure;

        private int _caterpillarsLeft;
        private int _buildsLeft;
        private Color _caterpillarColor = Color.White;

        internal SproutFigure(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _stemMesh = SceneMeshes.Build(graphicsDevice, AddStemWithNodes);
            _caterpillarLeafMesh = SceneMeshes.Build(graphicsDevice, AddCaterpillarLeaf);
            _budMesh = SceneMeshes.Build(graphicsDevice, SproutShapes.AddBud);
            _dewLeafFigure = new DewLeafFigure(graphicsDevice, _pointsLeafWorld);
            _caterpillarFigure = new CaterpillarFigure(graphicsDevice);
        }

        internal static Vector3 PointsLabelAnchor { get; } = SproutShapes.FindStemPointAtHeight(PointsLeafHeight) + _pointsLabelOffset;

        internal static Vector3 CaterpillarsLabelAnchor { get; } = SproutShapes.FindStemPointAtHeight(CaterpillarLeafHeight) + _caterpillarsLabelOffset;

        internal static Vector3 BuildsLabelAnchor { get; } = SproutShapes.StemTip + _buildsLabelOffset;

        internal void ShowActionPoints(int actionPoints, int previewCost, TimeSpan sceneTime)
        {
            _dewLeafFigure.ShowActionPoints(actionPoints, previewCost, sceneTime);
        }

        internal void ForgetActionPoints()
        {
            _dewLeafFigure.ForgetActionPoints();
        }

        internal void ShowReserve(int caterpillarsLeft, Color caterpillarColor, int buildsLeft)
        {
            _caterpillarsLeft = Math.Min(caterpillarsLeft, _caterpillarSpots.Length);
            _caterpillarColor = caterpillarColor;
            _buildsLeft = buildsLeft;
        }

        internal void Draw(BasicEffect sceneEffect, TimeSpan sceneTime)
        {
            sceneEffect.World = Matrix.Identity;
            _stemMesh.Draw(_graphicsDevice, sceneEffect);
            DrawBuds(sceneEffect);
            RasterizerState frontRasterizerState = _graphicsDevice.RasterizerState;
            _graphicsDevice.RasterizerState = RasterizerState.CullNone;
            sceneEffect.World = _caterpillarLeafWorld;
            _caterpillarLeafMesh.Draw(_graphicsDevice, sceneEffect);
            _graphicsDevice.RasterizerState = frontRasterizerState;
            _dewLeafFigure.Draw(sceneEffect, sceneTime);
            DrawCaterpillars(sceneEffect);
            sceneEffect.World = Matrix.Identity;
        }

        public void Dispose()
        {
            _stemMesh.Dispose();
            _caterpillarLeafMesh.Dispose();
            _budMesh.Dispose();
            _dewLeafFigure.Dispose();
            _caterpillarFigure.Dispose();
        }

        private static Matrix CreateLeafWorld(float attachHeight, float leafScale, float droopDegrees)
        {
            Matrix leafWorld = Matrix.CreateTranslation(DewLeafShapes.PetioleReach, 0f, 0f)
                * Matrix.CreateScale(leafScale)
                * Matrix.CreateRotationZ(MathHelper.ToRadians(droopDegrees))
                * Matrix.CreateTranslation(SproutShapes.FindStemPointAtHeight(attachHeight));

            return leafWorld;
        }

        private static void AddStemWithNodes(MeshBuilder meshBuilder)
        {
            SproutShapes.AddStem(meshBuilder);
            SproutShapes.AddNode(meshBuilder, SproutShapes.FindStemPointAtHeight(PointsLeafHeight), SproutShapes.FindStemRadiusAtHeight(PointsLeafHeight));
            SproutShapes.AddNode(meshBuilder, SproutShapes.FindStemPointAtHeight(CaterpillarLeafHeight), SproutShapes.FindStemRadiusAtHeight(CaterpillarLeafHeight));
        }

        private static float FindBudAngleDegrees(int budIndex, int budCount)
        {
            float budFraction = budCount > SingleBudCount ? (float)budIndex / (budCount - 1) : MiddleBudFraction;
            float budAngleDegrees = MathHelper.Lerp(FirstBudAngleDegrees, LastBudAngleDegrees, budFraction);

            return budAngleDegrees;
        }

        private void DrawBuds(BasicEffect sceneEffect)
        {
            for (var budIndex = 0; budIndex < _buildsLeft; budIndex++)
            {
                float budAngleRadians = MathHelper.ToRadians(FindBudAngleDegrees(budIndex, _buildsLeft));
                sceneEffect.World = Matrix.CreateRotationZ(budAngleRadians) * Matrix.CreateTranslation(SproutShapes.StemTip);
                _budMesh.Draw(_graphicsDevice, sceneEffect);
            }
        }

        private void DrawCaterpillars(BasicEffect sceneEffect)
        {
            for (var caterpillarIndex = 0; caterpillarIndex < _caterpillarsLeft; caterpillarIndex++)
            {
                Vector2 caterpillarSpot = _caterpillarSpots[caterpillarIndex];
                Vector3 leafPoint = DewLeafShapes.FindSurfacePoint(caterpillarSpot.X, caterpillarSpot.Y);
                Vector3 bodyOrigin = Vector3.Transform(leafPoint, _caterpillarLeafWorld) + new Vector3(-CaterpillarBackShift, CaterpillarLift, 0f);
                _caterpillarFigure.DrawStill(sceneEffect, new CaterpillarPose(bodyOrigin, CaterpillarHeadingDegrees, _caterpillarColor, 0f, CaterpillarScale));
            }
        }

        private static void AddCaterpillarLeaf(MeshBuilder leafBuilder)
        {
            DewLeafShapes.AddLeaf(leafBuilder, CaterpillarLeafSeed);
        }
    }
}
