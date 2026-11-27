using System;
using System.Collections.Generic;
using System.Text;

using FontStashSharp;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Torres.Client.Localization;
using Torres.Client.UI;

namespace Torres.Client.Scene
{
    internal sealed class CardFaceTextures : IDisposable
    {
        private const int FaceWidth = 192;
        private const int FaceHeight = 264;
        private const int BadgeSize = 40;
        private const int BadgeLineWidth = 3;
        private const float NameLeft = 58f;
        private const float NameRight = 182f;
        private const float NameBandTop = 6f;
        private const float NameBandHeight = 56f;
        private const float NameLineSpacing = 0f;
        private const string WordSeparator = " ";
        private const char WordSeparatorCharacter = ' ';

        private static readonly Vector2 _badgePosition = new Vector2(12f, 12f);
        private static readonly Rectangle _stripeBounds = new Rectangle(18, 249, 156, 7);

        private readonly GraphicsDevice _graphicsDevice;
        private readonly CardImages _cardImages;
        private readonly SpriteBatch _spriteBatch;
        private readonly Texture2D _badgeTexture;
        private readonly Texture2D _whitePixel;
        private readonly Dictionary<MatchCard, RenderTarget2D> _faces = new Dictionary<MatchCard, RenderTarget2D>();

        private bool _isRendered;

        internal CardFaceTextures(GraphicsDevice graphicsDevice, CardImages cardImages)
        {
            _graphicsDevice = graphicsDevice;
            _cardImages = cardImages;
            _spriteBatch = new SpriteBatch(graphicsDevice);
            _badgeTexture = CreateBadgeTexture(graphicsDevice);
            _whitePixel = new Texture2D(graphicsDevice, 1, 1);
            _whitePixel.SetData(new Color[] { Color.White });
            foreach (MatchCard card in Enum.GetValues<MatchCard>())
            {
                _faces[card] = new RenderTarget2D(graphicsDevice, FaceWidth, FaceHeight, false, SurfaceFormat.Color, DepthFormat.None, 0, RenderTargetUsage.PreserveContents);
            }
        }

        internal Texture2D FindFace(MatchCard card)
        {
            Texture2D face = _faces[card];

            return face;
        }

        internal void Invalidate()
        {
            _isRendered = false;
        }

        internal void RenderOnce()
        {
            if (_isRendered)
            {
                return;
            }

            foreach (KeyValuePair<MatchCard, RenderTarget2D> cardFace in _faces)
            {
                RenderFace(cardFace.Key, cardFace.Value);
            }

            _graphicsDevice.SetRenderTarget(null);
            _isRendered = true;
        }

        public void Dispose()
        {
            foreach (RenderTarget2D face in _faces.Values)
            {
                face.Dispose();
            }

            _spriteBatch.Dispose();
            _badgeTexture.Dispose();
            _whitePixel.Dispose();
        }

        private static Texture2D CreateBadgeTexture(GraphicsDevice graphicsDevice)
        {
            var badgeCenter = new Vector2(BadgeSize * SceneValues.HalfFactor);
            float badgeRadius = BadgeSize * SceneValues.HalfFactor;
            var pixels = new Color[BadgeSize * BadgeSize];
            for (var row = 0; row < BadgeSize; row++)
            {
                for (var column = 0; column < BadgeSize; column++)
                {
                    float centerDistance = Vector2.Distance(new Vector2(column + SceneValues.PixelCenter, row + SceneValues.PixelCenter), badgeCenter);
                    float coverage = MathHelper.Clamp(badgeRadius - centerDistance, 0f, 1f);
                    float lineCoverage = MathHelper.Clamp(centerDistance - (badgeRadius - BadgeLineWidth), 0f, 1f);
                    pixels[(row * BadgeSize) + column] = Color.Lerp(Theme.Surface, Theme.CardLine, lineCoverage) * coverage;
                }
            }

            var badgeTexture = new Texture2D(graphicsDevice, BadgeSize, BadgeSize);
            badgeTexture.SetData(pixels);

            return badgeTexture;
        }

        private static List<string> WrapName(SpriteFontBase nameFont, string cardName, float lineWidth)
        {
            var nameLines = new List<string>();
            var currentLine = new StringBuilder();
            foreach (string word in cardName.Split(WordSeparatorCharacter))
            {
                string candidateLine = currentLine.Length == 0 ? word : currentLine + WordSeparator + word;
                if ((currentLine.Length > 0) && (nameFont.MeasureString(candidateLine).X > lineWidth))
                {
                    nameLines.Add(currentLine.ToString());
                    currentLine.Clear();
                    currentLine.Append(word);
                }
                else
                {
                    currentLine.Clear();
                    currentLine.Append(candidateLine);
                }
            }

            nameLines.Add(currentLine.ToString());

            return nameLines;
        }

        private static Color FindStripeColor(MatchCard card)
        {
            Color stripeColor = card switch
            {
                MatchCard.SixActionPoints or MatchCard.SevenActionPoints => Theme.PointsCardStripe,
                MatchCard.FlowerLevelMove or MatchCard.FlowerLevelRemoval => Theme.FlowerCardStripe,
                MatchCard.DoubleClimb or MatchCard.GroundLeap or MatchCard.Relocation or MatchCard.PlanterCrossing => Theme.CaterpillarCardStripe,
                _ => throw new ArgumentOutOfRangeException(nameof(card)),
            };

            return stripeColor;
        }

        private void RenderFace(MatchCard card, RenderTarget2D faceTarget)
        {
            _graphicsDevice.SetRenderTarget(faceTarget);
            _graphicsDevice.Clear(Color.Transparent);
            _spriteBatch.Begin(SpriteSortMode.Deferred, BlendState.AlphaBlend, SamplerState.LinearClamp);
            _spriteBatch.Draw(_cardImages.FindFace(card), Vector2.Zero, Color.White);
            _spriteBatch.Draw(_badgeTexture, _badgePosition, Color.White);
            string cardNumber = DisplayFormats.FormatInteger(CardImages.FindCardNumber(card));
            SpriteFontBase numberFont = Fonts.Headline;
            Vector2 numberSize = numberFont.MeasureString(cardNumber);
            Vector2 numberPosition = _badgePosition + ((new Vector2(BadgeSize) - numberSize) * SceneValues.HalfFactor);
            _spriteBatch.DrawString(numberFont, cardNumber, numberPosition, Theme.Ink);
            DrawName(LocalizedText.Get(CardTexts.ToNameKey(card)));
            _spriteBatch.Draw(_whitePixel, _stripeBounds, FindStripeColor(card));
            _spriteBatch.End();
        }

        private void DrawName(string cardName)
        {
            SpriteFontBase nameFont = Fonts.Heading;
            List<string> nameLines = WrapName(nameFont, cardName, NameRight - NameLeft);
            float lineHeight = nameFont.LineHeight + NameLineSpacing;
            float lineTop = NameBandTop + ((NameBandHeight - (lineHeight * nameLines.Count)) * SceneValues.HalfFactor);
            foreach (string nameLine in nameLines)
            {
                float lineWidth = nameFont.MeasureString(nameLine).X;
                var linePosition = new Vector2(NameLeft + ((NameRight - NameLeft - lineWidth) * SceneValues.HalfFactor), lineTop);
                _spriteBatch.DrawString(nameFont, nameLine, linePosition, Theme.Ink);
                lineTop += lineHeight;
            }
        }
    }
}
