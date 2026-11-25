using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class DeckFigure : IDisposable
    {
        private const int MostShownLayers = 7;
        private const float DeckCardWidth = 0.84f;
        private const float DeckCardLength = 1.155f;
        private const float LayerThickness = 0.026f;
        private const float LayerStep = 0.03f;
        private const float FirstHeadingDegrees = -8f;
        private const float HeadingStepDegrees = 0.6f;
        private const float FaceLift = 0.002f;
        private const float LabelDistance = 0.75f;

        private static readonly Vector3 _deckPosition = new Vector3(3.6f, TableShapes.TableTop, 5.5f);

        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _layerMesh;
        private readonly TexturedQuad _backQuad;

        private int _shownLayers;

        internal DeckFigure(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            var layerBuilder = new MeshBuilder();
            layerBuilder.AddBox(Matrix.CreateScale(DeckCardWidth, LayerThickness, DeckCardLength), GardenPalette.DeckCardEdge);
            _layerMesh = layerBuilder.BuildMesh(graphicsDevice);
            _backQuad = new TexturedQuad(graphicsDevice, DeckCardWidth, DeckCardLength, true);
        }

        internal static Vector3 LabelAnchor { get; } = _deckPosition + new Vector3(0f, 0f, LabelDistance);

        internal static Matrix TopCardWorld { get; } = Matrix.CreateTranslation(0f, LayerThickness, 0f) * FindLayerWorld(MostShownLayers - 1);

        internal static float CardWidthShare { get; } = DeckCardWidth / HandFigure.CardWidth;

        internal void ShowDeck(int deckCount)
        {
            _shownLayers = Math.Min(deckCount, MostShownLayers);
        }

        internal void DrawBodies(BasicEffect sceneEffect)
        {
            for (var layerIndex = 0; layerIndex < _shownLayers; layerIndex++)
            {
                sceneEffect.World = FindLayerWorld(layerIndex);
                _layerMesh.Draw(_graphicsDevice, sceneEffect);
            }

            sceneEffect.World = Matrix.Identity;
        }

        internal void DrawTop(BasicEffect textureEffect, Texture2D backTexture)
        {
            if (_shownLayers == 0)
            {
                return;
            }

            textureEffect.Texture = backTexture;
            textureEffect.World = Matrix.CreateTranslation(0f, LayerThickness + FaceLift, 0f) * FindLayerWorld(_shownLayers - 1);
            _backQuad.Draw(_graphicsDevice, textureEffect);
        }

        public void Dispose()
        {
            _layerMesh.Dispose();
            _backQuad.Dispose();
        }

        private static Matrix FindLayerWorld(int layerIndex)
        {
            Matrix layerWorld = BoardGeometry.CreateHeadingRotation(FirstHeadingDegrees + (HeadingStepDegrees * layerIndex))
                * Matrix.CreateTranslation(_deckPosition + new Vector3(0f, LayerStep * layerIndex, 0f));

            return layerWorld;
        }
    }
}
