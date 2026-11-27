using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class TexturedQuad : IDisposable
    {
        private const int CornerCount = 4;
        private const int TriangleCount = 2;
        private const float FrontSideSign = 1f;
        private const float BackSideSign = -1f;

        private static readonly short[] _quadIndices = new short[] { 0, 1, 2, 0, 2, 3 };

        private readonly VertexBuffer _vertexBuffer;
        private readonly IndexBuffer _indexBuffer;

        internal TexturedQuad(GraphicsDevice graphicsDevice, float quadWidth, float quadDepth, bool isFacingUp)
        {
            float halfWidth = quadWidth * SceneValues.HalfFactor;
            float halfDepth = quadDepth * SceneValues.HalfFactor;
            Vector3 quadNormal = isFacingUp ? Vector3.Up : Vector3.Down;
            float sideSign = isFacingUp ? FrontSideSign : BackSideSign;
            var corners = new VertexPositionNormalTexture[]
            {
                new VertexPositionNormalTexture(new Vector3(-halfWidth * sideSign, 0f, -halfDepth), quadNormal, new Vector2(0f, 0f)),
                new VertexPositionNormalTexture(new Vector3(halfWidth * sideSign, 0f, -halfDepth), quadNormal, new Vector2(1f, 0f)),
                new VertexPositionNormalTexture(new Vector3(halfWidth * sideSign, 0f, halfDepth), quadNormal, new Vector2(1f, 1f)),
                new VertexPositionNormalTexture(new Vector3(-halfWidth * sideSign, 0f, halfDepth), quadNormal, new Vector2(0f, 1f)),
            };
            _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalTexture.VertexDeclaration, CornerCount, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(corners);
            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.SixteenBits, _quadIndices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(_quadIndices);
        }

        internal void Draw(GraphicsDevice graphicsDevice, Effect effect)
        {
            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, TriangleCount);
            }
        }

        public void Dispose()
        {
            _vertexBuffer.Dispose();
            _indexBuffer.Dispose();
        }
    }
}
