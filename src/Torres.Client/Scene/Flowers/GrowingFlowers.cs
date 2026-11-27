using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class GrowingFlowers : IDisposable
    {
        private const double GrowSeconds = 0.55;
        private const float SproutStartFraction = 0.05f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly List<GrowingFlower> _flowers = new List<GrowingFlower>();

        internal GrowingFlowers(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
        }

        internal IReadOnlyCollection<BoardCell> Cells => _flowers.Select(flower => flower.Cell).ToList();

        internal void Replace(LayoutChange layoutChange, TimeSpan sceneTime)
        {
            Clear();
            foreach (BoardCell examinedCell in BoardLayout.EnumerateCells())
            {
                int previousLevel = layoutChange.PreviousLayout.GetFlowerLevel(examinedCell);
                int nextLevel = layoutChange.NextLayout.GetFlowerLevel(examinedCell);
                if (nextLevel > previousLevel)
                {
                    var flowerBuilder = new MeshBuilder();
                    FlowerShapes.AddCenteredFlower(flowerBuilder, layoutChange.NextLayout, examinedCell);
                    bool isNewFlower = previousLevel == 0;
                    float startHeightFraction = isNewFlower ? SproutStartFraction : BoardGeometry.GetFlowerTop(previousLevel) / BoardGeometry.GetFlowerTop(nextLevel);
                    _flowers.Add(new GrowingFlower(examinedCell, flowerBuilder.BuildMesh(_graphicsDevice), sceneTime, startHeightFraction, isNewFlower));
                }
            }
        }

        internal void Clear()
        {
            foreach (GrowingFlower flower in _flowers)
            {
                flower.Mesh.Dispose();
            }

            _flowers.Clear();
        }

        internal void Draw(BasicEffect sceneEffect, TimeSpan sceneTime)
        {
            foreach (GrowingFlower flower in _flowers)
            {
                float growth = SceneTiming.EaseOutWithOvershoot(SceneTiming.FindProgress(sceneTime, flower.StartTime, GrowSeconds));
                float heightScale = MathHelper.Lerp(flower.StartHeightFraction, SceneValues.FullScale, growth);
                float widthScale = flower.IsNewFlower ? heightScale : SceneValues.FullScale;
                Vector3 soilPoint = BoardGeometry.GetCellCenter(flower.Cell) + new Vector3(0f, BoardGeometry.SoilTop, 0f);
                sceneEffect.World = Matrix.CreateTranslation(0f, -BoardGeometry.SoilTop, 0f)
                    * Matrix.CreateScale(widthScale, heightScale, widthScale)
                    * Matrix.CreateTranslation(soilPoint);
                flower.Mesh.Draw(_graphicsDevice, sceneEffect);
            }

            sceneEffect.World = Matrix.Identity;
        }

        public void Dispose()
        {
            Clear();
        }

        private sealed record GrowingFlower(BoardCell Cell, StaticMesh Mesh, TimeSpan StartTime, float StartHeightFraction, bool IsNewFlower);
    }
}
