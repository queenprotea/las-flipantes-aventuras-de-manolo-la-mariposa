using System.Runtime.InteropServices;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    [StructLayout(LayoutKind.Sequential)]
    internal readonly struct VertexPositionNormalColor : IVertexType
    {
        private const int PositionOffset = 0;
        private const int NormalOffset = 12;
        private const int ColorOffset = 24;

        internal readonly Vector3 Position;
        internal readonly Vector3 Normal;
        internal readonly Color Color;

        internal VertexPositionNormalColor(Vector3 position, Vector3 normal, Color color)
        {
            Position = position;
            Normal = normal;
            Color = color;
        }

        internal static VertexDeclaration Declaration { get; } = new VertexDeclaration(
            new VertexElement(PositionOffset, VertexElementFormat.Vector3, VertexElementUsage.Position, 0),
            new VertexElement(NormalOffset, VertexElementFormat.Vector3, VertexElementUsage.Normal, 0),
            new VertexElement(ColorOffset, VertexElementFormat.Color, VertexElementUsage.Color, 0));

        VertexDeclaration IVertexType.VertexDeclaration => Declaration;
    }
}
