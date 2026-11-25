using System;
using System.Collections.Generic;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Torres.Client.Ui;

namespace Torres.Client.Scene
{
    internal sealed class HandLayer : IDisposable
    {
        private const float FarPlaneDepth = 1f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly OrbitCamera _handCamera = new OrbitCamera();
        private readonly HandFigure _handFigure;
        private readonly DeckFigure _deckFigure;
        private readonly CardImages _cardImages;
        private readonly CardFaceTextures _cardFaces;
        private readonly BasicEffect _textureEffect;

        internal HandLayer(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _handFigure = new HandFigure(graphicsDevice);
            _deckFigure = new DeckFigure(graphicsDevice);
            _cardImages = new CardImages(graphicsDevice);
            _cardFaces = new CardFaceTextures(graphicsDevice, _cardImages);
            _textureEffect = new BasicEffect(graphicsDevice)
            {
                TextureEnabled = true,
                VertexColorEnabled = false,
                PreferPerPixelLighting = true,
                SpecularColor = Vector3.Zero,
                LightingEnabled = true,
            };
            SceneLighting.ApplyTo(_textureEffect);
        }

        internal Matrix ViewMatrix => _handCamera.CreateViewMatrix();

        internal MatchCard? HoveredCard
        {
            get => _handFigure.HoveredCard;
            set => _handFigure.HoveredCard = value;
        }

        internal void ShowHand(IReadOnlyList<HandCardState> handStates, int deckCount)
        {
            _handFigure.ShowHand(handStates);
            _deckFigure.ShowDeck(deckCount);
        }

        internal void ForgetHand()
        {
            _handFigure.ForgetHand();
        }

        internal void RefreshCardFaces()
        {
            _cardFaces.Invalidate();
        }

        internal void Advance(TimeSpan sceneTime)
        {
            _handFigure.SceneTime = sceneTime;
        }

        internal void RenderFacesOnce()
        {
            _cardFaces.RenderOnce();
        }

        internal MatchCard? PickCard(Ray pickRay)
        {
            MatchCard? pickedCard = _handFigure.PickCard(pickRay);

            return pickedCard;
        }

        internal void Draw(BasicEffect sceneEffect)
        {
            _graphicsDevice.Clear(ClearOptions.DepthBuffer, Color.Transparent, FarPlaneDepth, 0);
            _graphicsDevice.BlendState = BlendState.Opaque;
            _graphicsDevice.DepthStencilState = DepthStencilState.Default;
            _graphicsDevice.RasterizerState = RasterizerState.CullCounterClockwise;
            sceneEffect.View = ViewMatrix;
            sceneEffect.World = Matrix.Identity;
            sceneEffect.LightingEnabled = true;
            _deckFigure.DrawBodies(sceneEffect);
            _handFigure.DrawBodies(sceneEffect);
            _textureEffect.View = sceneEffect.View;
            _textureEffect.Projection = sceneEffect.Projection;
            _graphicsDevice.BlendState = BlendState.AlphaBlend;
            _graphicsDevice.SamplerStates[0] = SamplerState.LinearClamp;
            _deckFigure.DrawTop(_textureEffect, _cardImages.BackTexture);
            _handFigure.DrawFaces(_textureEffect, _cardFaces);
            _graphicsDevice.BlendState = BlendState.Opaque;
        }

        public void Dispose()
        {
            _handFigure.Dispose();
            _deckFigure.Dispose();
            _cardFaces.Dispose();
            _cardImages.Dispose();
            _textureEffect.Dispose();
        }
    }
}
