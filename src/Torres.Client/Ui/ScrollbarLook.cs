using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Myra;
using Myra.Graphics2D;
using Myra.Graphics2D.TextureAtlases;
using Myra.Graphics2D.UI;

namespace Torres.Client.Ui
{
    internal static class ScrollbarLook
    {
        private const int FlatTextureHeight = 1;

        private static IImage? _trackImage;
        private static IImage? _knobImage;

        internal static void Apply(ScrollViewer scrollViewer)
        {
            _trackImage ??= CreateFlatImage(Theme.Line);
            _knobImage ??= CreateFlatImage(Theme.MintLine);
            scrollViewer.VerticalScrollBackground = _trackImage;
            scrollViewer.VerticalScrollKnob = _knobImage;
        }

        private static IImage CreateFlatImage(Color imageColor)
        {
            var whiteTexture = new Texture2D(MyraEnvironment.GraphicsDevice, Metrics.ScrollbarWidth, FlatTextureHeight);
            var whitePixels = new Color[Metrics.ScrollbarWidth * FlatTextureHeight];
            Array.Fill(whitePixels, Color.White);
            whiteTexture.SetData(whitePixels);
            var flatImage = new ColoredRegion(new TextureRegion(whiteTexture), imageColor);

            return flatImage;
        }
    }
}
