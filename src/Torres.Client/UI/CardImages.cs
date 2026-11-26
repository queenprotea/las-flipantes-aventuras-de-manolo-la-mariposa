using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.UI
{
    internal sealed class CardImages : IDisposable
    {
        private const string FaceResourcePattern = "Cards.carta-{0}.png";
        private const string BackResourceName = "Cards.carta-reverso-menta.png";
        private const string MissingCardImageMessage = "The card image {0} is not embedded in the client.";
        private const int FirstCardNumber = 1;

        private readonly List<Texture2D> _textures = new List<Texture2D>();
        private readonly Dictionary<MatchCard, Texture2D> _faces = new Dictionary<MatchCard, Texture2D>();

        internal CardImages(GraphicsDevice graphicsDevice)
        {
            foreach (MatchCard card in Enum.GetValues<MatchCard>())
            {
                string resourceName = string.Format(CultureInfo.InvariantCulture, FaceResourcePattern, FindCardNumber(card));
                _faces[card] = LoadTexture(graphicsDevice, resourceName);
            }

            BackTexture = LoadTexture(graphicsDevice, BackResourceName);
        }

        internal Texture2D BackTexture { get; }

        internal static int FindCardNumber(MatchCard card)
        {
            int cardNumber = (int)card + FirstCardNumber;

            return cardNumber;
        }

        internal Texture2D FindFace(MatchCard card)
        {
            Texture2D face = _faces[card];

            return face;
        }

        public void Dispose()
        {
            foreach (Texture2D texture in _textures)
            {
                texture.Dispose();
            }
        }

        private static void PremultiplyAlpha(Texture2D texture)
        {
            var pixels = new Color[texture.Width * texture.Height];
            texture.GetData(pixels);
            for (var i = 0; i < pixels.Length; i++)
            {
                pixels[i] = Color.FromNonPremultiplied(pixels[i].R, pixels[i].G, pixels[i].B, pixels[i].A);
            }

            texture.SetData(pixels);
        }

        private Texture2D LoadTexture(GraphicsDevice graphicsDevice, string resourceName)
        {
            Assembly clientAssembly = typeof(CardImages).Assembly;
            using Stream? imageStream = clientAssembly.GetManifestResourceStream(resourceName);
            if (imageStream is null)
            {
                throw new InvalidOperationException(string.Format(CultureInfo.InvariantCulture, MissingCardImageMessage, resourceName));
            }

            Texture2D texture = Texture2D.FromStream(graphicsDevice, imageStream);
            PremultiplyAlpha(texture);
            _textures.Add(texture);

            return texture;
        }
    }
}
