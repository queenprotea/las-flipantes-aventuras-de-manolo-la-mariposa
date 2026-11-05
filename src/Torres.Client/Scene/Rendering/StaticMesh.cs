using System;

using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class StaticMesh : IDisposable
    {
        private const int TriangleCornerCount = 3;

        private readonly VertexBuffer? _vertexBuffer;
        private readonly IndexBuffer? _indexBuffer;
        private readonly int _triangleCount;

        internal StaticMesh(GraphicsDevice graphicsDevice, VertexPositionNormalColor[] vertices, int[] indices)
        {
            if (vertices.Length == 0)
            {
                return;
            }

            _vertexBuffer = new VertexBuffer(graphicsDevice, VertexPositionNormalColor.Declaration, vertices.Length, BufferUsage.WriteOnly);
            _vertexBuffer.SetData(vertices);
            _indexBuffer = new IndexBuffer(graphicsDevice, IndexElementSize.ThirtyTwoBits, indices.Length, BufferUsage.WriteOnly);
            _indexBuffer.SetData(indices);
            _triangleCount = indices.Length / TriangleCornerCount;
        }

        internal void Draw(GraphicsDevice graphicsDevice, Effect effect)
        {
            if ((_vertexBuffer is null) || (_indexBuffer is null))
            {
                return;
            }

            graphicsDevice.SetVertexBuffer(_vertexBuffer);
            graphicsDevice.Indices = _indexBuffer;
            foreach (EffectPass effectPass in effect.CurrentTechnique.Passes)
            {
                effectPass.Apply();
                graphicsDevice.DrawIndexedPrimitives(PrimitiveType.TriangleList, 0, 0, _triangleCount);
            }
        }

        public void Dispose()
        {
            _vertexBuffer?.Dispose();
            _indexBuffer?.Dispose();
        }
    }
}
