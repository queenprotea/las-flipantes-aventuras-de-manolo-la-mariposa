using System;
using System.Collections.Generic;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace Torres.Client.Scene
{
    internal sealed class MeshBuilder
    {
        private const int CircleSegments = 16;
        private const int SphereRings = 10;
        private const int RingVertexCount = CircleSegments + 1;
        private const int CylinderVerticesPerSegment = 3;
        private const int CylinderSideTopOffset = 1;
        private const int CylinderCapEdgeOffset = 2;
        private const float HalfUnit = 0.5f;

        private static readonly Vector3 _boxCenter = new Vector3(0f, HalfUnit, 0f);
        private static readonly Vector3[] _boxFaceNormals = new Vector3[]
        {
            Vector3.Up, Vector3.Down, Vector3.Left, Vector3.Right, Vector3.Forward, Vector3.Backward,
        };

        private readonly List<VertexPositionNormalColor> _vertices = new List<VertexPositionNormalColor>();
        private readonly List<int> _indices = new List<int>();

        internal void AddBox(Matrix transform, Color color)
        {
            var meshPlacement = new MeshPlacement(transform, color);
            foreach (Vector3 faceNormal in _boxFaceNormals)
            {
                AddBoxFace(faceNormal, meshPlacement);
            }
        }

        internal void AddCylinder(Matrix transform, Color color)
        {
            var meshPlacement = new MeshPlacement(transform, color);
            int capCenterIndex = AddVertex(new SurfacePoint(Vector3.Up, Vector3.Up), meshPlacement);
            int firstSegmentVertexIndex = _vertices.Count;
            for (var segmentIndex = 0; segmentIndex <= CircleSegments; segmentIndex++)
            {
                Vector3 radialDirection = CreateRadialDirection(segmentIndex);
                Vector3 topPoint = radialDirection + Vector3.Up;
                AddVertex(new SurfacePoint(radialDirection, radialDirection), meshPlacement);
                AddVertex(new SurfacePoint(topPoint, radialDirection), meshPlacement);
                AddVertex(new SurfacePoint(topPoint, Vector3.Up), meshPlacement);
            }

            for (var segmentIndex = 0; segmentIndex < CircleSegments; segmentIndex++)
            {
                int bottomVertexIndex = firstSegmentVertexIndex + (segmentIndex * CylinderVerticesPerSegment);
                int nextBottomVertexIndex = bottomVertexIndex + CylinderVerticesPerSegment;
                AddTriangle(bottomVertexIndex, nextBottomVertexIndex, nextBottomVertexIndex + CylinderSideTopOffset);
                AddTriangle(bottomVertexIndex, nextBottomVertexIndex + CylinderSideTopOffset, bottomVertexIndex + CylinderSideTopOffset);
                AddTriangle(capCenterIndex, bottomVertexIndex + CylinderCapEdgeOffset, nextBottomVertexIndex + CylinderCapEdgeOffset);
            }
        }

        internal void AddSphere(Matrix transform, Color color)
        {
            var meshPlacement = new MeshPlacement(transform, color);
            int firstVertex = _vertices.Count;
            for (var ringIndex = 0; ringIndex <= SphereRings; ringIndex++)
            {
                AddSphereRing(ringIndex, meshPlacement);
            }

            for (var ringIndex = 0; ringIndex < SphereRings; ringIndex++)
            {
                AddSphereBand(firstVertex + (ringIndex * RingVertexCount));
            }
        }

        internal void AddDisc(Matrix transform, Color color)
        {
            var meshPlacement = new MeshPlacement(transform, color);
            int discCenterIndex = AddVertex(new SurfacePoint(Vector3.Zero, Vector3.Up), meshPlacement);
            int firstEdgeVertexIndex = _vertices.Count;
            for (var segmentIndex = 0; segmentIndex <= CircleSegments; segmentIndex++)
            {
                AddVertex(new SurfacePoint(CreateRadialDirection(segmentIndex), Vector3.Up), meshPlacement);
            }

            for (var segmentIndex = 0; segmentIndex < CircleSegments; segmentIndex++)
            {
                int edgeVertexIndex = firstEdgeVertexIndex + segmentIndex;
                AddTriangle(discCenterIndex, edgeVertexIndex, edgeVertexIndex + 1);
            }
        }

        internal StaticMesh BuildMesh(GraphicsDevice graphicsDevice)
        {
            var mesh = new StaticMesh(graphicsDevice, _vertices.ToArray(), _indices.ToArray());

            return mesh;
        }

        private static Vector3 CreateRadialDirection(int segmentIndex)
        {
            float angleRadians = MathHelper.TwoPi * segmentIndex / CircleSegments;
            var radialDirection = new Vector3(MathF.Cos(angleRadians), 0f, MathF.Sin(angleRadians));

            return radialDirection;
        }

        private void AddBoxFace(Vector3 faceNormal, MeshPlacement meshPlacement)
        {
            var firstAxis = new Vector3(faceNormal.Y, faceNormal.Z, faceNormal.X);
            Vector3 secondAxis = Vector3.Cross(faceNormal, firstAxis);
            Vector3 faceCenter = _boxCenter + (faceNormal * HalfUnit);
            Vector3 diagonal = (firstAxis + secondAxis) * HalfUnit;
            Vector3 antidiagonal = (firstAxis - secondAxis) * HalfUnit;
            int firstCornerIndex = AddVertex(new SurfacePoint(faceCenter - diagonal, faceNormal), meshPlacement);
            int secondCornerIndex = AddVertex(new SurfacePoint(faceCenter + antidiagonal, faceNormal), meshPlacement);
            int thirdCornerIndex = AddVertex(new SurfacePoint(faceCenter + diagonal, faceNormal), meshPlacement);
            int fourthCornerIndex = AddVertex(new SurfacePoint(faceCenter - antidiagonal, faceNormal), meshPlacement);
            AddTriangle(firstCornerIndex, secondCornerIndex, thirdCornerIndex);
            AddTriangle(firstCornerIndex, thirdCornerIndex, fourthCornerIndex);
        }

        private void AddSphereRing(int ringIndex, MeshPlacement meshPlacement)
        {
            float polarAngleRadians = MathHelper.Pi * ringIndex / SphereRings;
            for (var segmentIndex = 0; segmentIndex <= CircleSegments; segmentIndex++)
            {
                Vector3 radialDirection = CreateRadialDirection(segmentIndex);
                Vector3 spherePoint = (radialDirection * MathF.Sin(polarAngleRadians)) + (Vector3.Up * MathF.Cos(polarAngleRadians));
                AddVertex(new SurfacePoint(spherePoint, spherePoint), meshPlacement);
            }
        }

        private void AddSphereBand(int firstUpperVertexIndex)
        {
            for (var segmentIndex = 0; segmentIndex < CircleSegments; segmentIndex++)
            {
                int upperLeftIndex = firstUpperVertexIndex + segmentIndex;
                int lowerLeftIndex = upperLeftIndex + RingVertexCount;
                AddTriangle(upperLeftIndex, lowerLeftIndex, lowerLeftIndex + 1);
                AddTriangle(upperLeftIndex, lowerLeftIndex + 1, upperLeftIndex + 1);
            }
        }

        private int AddVertex(SurfacePoint localPoint, MeshPlacement meshPlacement)
        {
            Vector3 position = Vector3.Transform(localPoint.Position, meshPlacement.Transform);
            Vector3 normal = Vector3.Normalize(Vector3.TransformNormal(localPoint.Normal, meshPlacement.NormalTransform));
            _vertices.Add(new VertexPositionNormalColor(position, normal, meshPlacement.Color));
            int vertexIndex = _vertices.Count - 1;

            return vertexIndex;
        }

        private void AddTriangle(int firstVertexIndex, int secondVertexIndex, int thirdVertexIndex)
        {
            Vector3 firstPosition = _vertices[firstVertexIndex].Position;
            Vector3 windingNormal = Vector3.Cross(_vertices[secondVertexIndex].Position - firstPosition, _vertices[thirdVertexIndex].Position - firstPosition);
            Vector3 outwardNormal = _vertices[firstVertexIndex].Normal + _vertices[secondVertexIndex].Normal + _vertices[thirdVertexIndex].Normal;
            bool isClockwiseFromOutside = Vector3.Dot(windingNormal, outwardNormal) < 0f;
            _indices.Add(firstVertexIndex);
            if (isClockwiseFromOutside)
            {
                _indices.Add(secondVertexIndex);
                _indices.Add(thirdVertexIndex);
            }
            else
            {
                _indices.Add(thirdVertexIndex);
                _indices.Add(secondVertexIndex);
            }
        }
    }
}
