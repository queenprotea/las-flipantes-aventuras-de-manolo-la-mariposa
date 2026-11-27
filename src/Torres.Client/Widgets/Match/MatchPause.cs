using System;

using Game.Contracts;

using Microsoft.Xna.Framework.Graphics;

using Myra.Graphics2D.UI;

using Torres.Client.Scene;
using Torres.Client.UI;

namespace Torres.Client.Widgets
{
    internal sealed class MatchPause : IDisposable
    {
        private readonly MatchBoardLayoutFactory _layoutFactory = new MatchBoardLayoutFactory();
        private readonly TableBackdrop _tableBackdrop;

        internal MatchPause(GraphicsDevice graphicsDevice)
        {
            VolumeTextures = new VolumeTextures(graphicsDevice);
            FigureIcons = new FigureIcons(graphicsDevice);
            _tableBackdrop = new TableBackdrop(graphicsDevice);
        }

        internal VolumeTextures VolumeTextures { get; }

        internal FigureIcons FigureIcons { get; }

        internal void ShowTable(Widget cardLayer, MatchView? matchView)
        {
            _tableBackdrop.RemoveFromParent();
            if (cardLayer is Panel layerPanel)
            {
                layerPanel.Widgets.Insert(0, _tableBackdrop);
            }

            if (matchView is null)
            {
                _tableBackdrop.ShowGarden(BoardLayout.Empty, null);
                return;
            }

            BoardCell? butterflyCell = null;
            if (matchView.HasButterfly)
            {
                butterflyCell = MatchCellConversion.ToBoardCell(matchView.ButterflyCell);
            }

            _tableBackdrop.ShowGarden(_layoutFactory.Create(matchView), butterflyCell);
        }

        internal void Advance(TimeSpan elapsedTime)
        {
            _tableBackdrop.Advance(elapsedTime);
        }

        internal void RenderScene()
        {
            FigureIcons.RenderOnce();
            _tableBackdrop.RenderScene();
        }

        public void Dispose()
        {
            _tableBackdrop.Dispose();
            FigureIcons.Dispose();
            VolumeTextures.Dispose();
        }
    }
}
