using System;
using System.Collections.Generic;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class HandFigure : IDisposable
    {
        internal const float CardWidth = 1.1f;
        internal const float CardLength = 1.52f;
        internal const float CardThickness = 0.036f;

        private const float TiltDegrees = 48f;
        private const float FanStepDegrees = 14f;
        private const float HandCenterDepth = 6.3f;
        private const float HandHeight = 0.75f;
        private const float CardSpacing = 1.25f;
        private const float HandSpan = 6.2f;
        private const float OuterCardPull = 0.08f;
        private const float StackStep = 0.04f;
        private const float HoverLift = 0.28f;
        private const float ChosenLift = 0.42f;
        private const float OutlineGrowth = 1.08f;
        private const float OutlineDrop = 0.012f;
        private const float UnplayableShade = 0.86f;
        private const float BodyInset = 0.94f;
        private const float FaceLift = 0.002f;
        private const float PickTopAllowance = 0.01f;
        private const float HalfShare = 0.5f;
        private const double FlightSeconds = 0.5;
        private const float FlightArcHeight = 0.7f;
        private const float FullScale = 1f;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly TexturedQuad _faceQuad;
        private readonly StaticMesh _bodyMesh;
        private readonly StaticMesh _outlineMesh;
        private readonly List<HandCardState> _handStates = new List<HandCardState>();
        private readonly Dictionary<MatchCard, TimeSpan> _arrivalTimes = new Dictionary<MatchCard, TimeSpan>();

        private bool _hasShownHand;

        internal HandFigure(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;
            _faceQuad = new TexturedQuad(graphicsDevice, CardWidth, CardLength, true);
            _bodyMesh = BuildSlab(graphicsDevice, BodyInset, GardenPalette.CardEdge);
            _outlineMesh = BuildSlab(graphicsDevice, OutlineGrowth, GardenPalette.ChosenCardOutline);
        }

        internal MatchCard? HoveredCard { get; set; }

        internal TimeSpan SceneTime { get; set; }

        internal void ShowHand(IReadOnlyList<HandCardState> handStates)
        {
            foreach (HandCardState handState in handStates)
            {
                bool isNewCard = !_handStates.Exists(shownState => shownState.Card == handState.Card);
                if (_hasShownHand && isNewCard)
                {
                    _arrivalTimes[handState.Card] = SceneTime;
                }
            }

            _handStates.Clear();
            _handStates.AddRange(handStates);
            _hasShownHand = true;
        }

        internal void ForgetHand()
        {
            _handStates.Clear();
            _arrivalTimes.Clear();
            _hasShownHand = false;
        }

        internal MatchCard? PickCard(Ray pickRay)
        {
            MatchCard? pickedCard = null;
            float nearestDistance = float.MaxValue;
            var cardBounds = new BoundingBox(new Vector3(-CardWidth * HalfShare, -CardThickness, -CardLength * HalfShare), new Vector3(CardWidth * HalfShare, PickTopAllowance, CardLength * HalfShare));
            for (var cardIndex = 0; cardIndex < _handStates.Count; cardIndex++)
            {
                Matrix localTransform = Matrix.Invert(FindCardWorld(cardIndex));
                var localRay = new Ray(Vector3.Transform(pickRay.Position, localTransform), Vector3.Normalize(Vector3.TransformNormal(pickRay.Direction, localTransform)));
                float? hitDistance = localRay.Intersects(cardBounds);
                if (hitDistance.HasValue && (hitDistance.Value < nearestDistance))
                {
                    nearestDistance = hitDistance.Value;
                    pickedCard = _handStates[cardIndex].Card;
                }
            }

            return pickedCard;
        }

        internal void DrawBodies(BasicEffect sceneEffect)
        {
            for (var cardIndex = 0; cardIndex < _handStates.Count; cardIndex++)
            {
                Matrix cardWorld = FindCardWorld(cardIndex);
                if (_handStates[cardIndex].IsChosen)
                {
                    sceneEffect.World = Matrix.CreateTranslation(0f, -OutlineDrop, 0f) * cardWorld;
                    _outlineMesh.Draw(_graphicsDevice, sceneEffect);
                }

                sceneEffect.World = cardWorld;
                _bodyMesh.Draw(_graphicsDevice, sceneEffect);
            }

            sceneEffect.World = Matrix.Identity;
        }

        internal void DrawFaces(BasicEffect textureEffect, CardFaceTextures cardFaces)
        {
            for (var cardIndex = 0; cardIndex < _handStates.Count; cardIndex++)
            {
                HandCardState handState = _handStates[cardIndex];
                textureEffect.Texture = cardFaces.FindFace(handState.Card);
                textureEffect.DiffuseColor = handState.IsPlayable ? Vector3.One : new Vector3(UnplayableShade);
                textureEffect.World = Matrix.CreateTranslation(0f, FaceLift, 0f) * FindCardWorld(cardIndex);
                _faceQuad.Draw(_graphicsDevice, textureEffect);
            }

            textureEffect.DiffuseColor = Vector3.One;
        }

        public void Dispose()
        {
            _faceQuad.Dispose();
            _bodyMesh.Dispose();
            _outlineMesh.Dispose();
        }

        private static StaticMesh BuildSlab(GraphicsDevice graphicsDevice, float slabGrowth, Color slabColor)
        {
            var slabBuilder = new MeshBuilder();
            slabBuilder.AddBox(Matrix.CreateScale(CardWidth * slabGrowth, CardThickness, CardLength * slabGrowth) * Matrix.CreateTranslation(0f, -CardThickness, 0f), slabColor);
            StaticMesh slabMesh = slabBuilder.BuildMesh(graphicsDevice);

            return slabMesh;
        }

        private Matrix FindCardWorld(int cardIndex)
        {
            float centerIndex = (_handStates.Count - 1) * HalfShare;
            float cardSpacing = _handStates.Count > 1 ? Math.Min(CardSpacing, HandSpan / (_handStates.Count - 1)) : CardSpacing;
            float centerOffset = cardIndex - centerIndex;
            HandCardState handState = _handStates[cardIndex];
            float cardLift = 0f;
            if (handState.IsChosen)
            {
                cardLift = ChosenLift;
            }
            else if (HoveredCard == handState.Card)
            {
                cardLift = HoverLift;
            }

            var cardPosition = new Vector3(centerOffset * cardSpacing, HandHeight + (StackStep * cardIndex) + cardLift, HandCenterDepth + (Math.Abs(centerOffset) * OuterCardPull));
            Matrix cardWorld = Matrix.CreateRotationX(MathHelper.ToRadians(TiltDegrees))
                * BoardGeometry.CreateHeadingRotation(-FanStepDegrees * centerOffset)
                * Matrix.CreateTranslation(cardPosition);
            if (_arrivalTimes.TryGetValue(handState.Card, out TimeSpan arrivalTime))
            {
                cardWorld = FindFlightWorld(cardWorld, SceneTiming.FindProgress(SceneTime, arrivalTime, FlightSeconds));
            }

            return cardWorld;
        }

        private static Matrix FindFlightWorld(Matrix handWorld, float flightProgress)
        {
            float easedProgress = MathHelper.SmoothStep(0f, FullScale, flightProgress);
            DeckFigure.TopCardWorld.Decompose(out _, out Quaternion deckRotation, out Vector3 deckPosition);
            handWorld.Decompose(out _, out Quaternion handRotation, out Vector3 handPosition);
            Vector3 flightPosition = Vector3.Lerp(deckPosition, handPosition, easedProgress) + new Vector3(0f, SceneTiming.FindArcLift(FlightArcHeight, easedProgress), 0f);
            float flightScale = MathHelper.Lerp(DeckFigure.CardWidthShare, FullScale, easedProgress);
            Matrix flightWorld = Matrix.CreateScale(flightScale)
                * Matrix.CreateFromQuaternion(Quaternion.Slerp(deckRotation, handRotation, easedProgress))
                * Matrix.CreateTranslation(flightPosition);

            return flightWorld;
        }
    }
}
