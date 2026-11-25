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
        private const int CoarseCircleSegments = 8;
        private const int CoarseSphereRings = 4;
        private const int RingSeamVertexCount = 1;
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
                Vector3 radialDirection = CreateRadialDirection(segmentIndex, CircleSegments);
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
            AddSphereOf(new MeshPlacement(transform, color), CircleSegments, SphereRings);
        }

        internal void AddCoarseSphere(Matrix transform, Color color)
        {
            AddSphereOf(new MeshPlacement(transform, color), CoarseCircleSegments, CoarseSphereRings);
        }

        internal void AddDisc(Matrix transform, Color color)
        {
            var meshPlacement = new MeshPlacement(transform, color);
            int discCenterIndex = AddVertex(new SurfacePoint(Vector3.Zero, Vector3.Up), meshPlacement);
            int firstEdgeVertexIndex = _vertices.Count;
            for (var segmentIndex = 0; segmentIndex <= CircleSegments; segmentIndex++)
            {
                AddVertex(new SurfacePoint(CreateRadialDirection(segmentIndex, CircleSegments), Vector3.Up), meshPlacement);
            }

            for (var segmentIndex = 0; segmentIndex < CircleSegments; segmentIndex++)
            {
                int edgeVertexIndex = firstEdgeVertexIndex + segmentIndex;
                AddTriangle(discCenterIndex, edgeVertexIndex, edgeVertexIndex + 1);
            }
        }

        internal void AddSurfaceGrid(IReadOnlyList<Vector3> gridPositions, IReadOnlyList<Color> gridColors, int rowLength)
        {
            int firstVertexIndex = _vertices.Count;
            int rowCount = gridPositions.Count / rowLength;
            for (var gridIndex = 0; gridIndex < gridPositions.Count; gridIndex++)
            {
                Vector3 gridNormal = FindGridNormal(gridPositions, gridIndex, rowLength, rowCount);
                _vertices.Add(new VertexPositionNormalColor(gridPositions[gridIndex], gridNormal, gridColors[gridIndex]));
            }

            for (var row = 0; row < rowCount - 1; row++)
            {
                for (var column = 0; column < rowLength - 1; column++)
                {
                    int cornerIndex = firstVertexIndex + (row * rowLength) + column;
                    AddTriangle(cornerIndex, cornerIndex + rowLength, cornerIndex + rowLength + 1);
                    AddTriangle(cornerIndex, cornerIndex + rowLength + 1, cornerIndex + 1);
                }
            }
        }

        internal StaticMesh BuildMesh(GraphicsDevice graphicsDevice)
        {
            var mesh = new StaticMesh(graphicsDevice, _vertices.ToArray(), _indices.ToArray());

            return mesh;
        }

        private static Vector3 CreateRadialDirection(int segmentIndex, int segmentCount)
        {
            float angleRadians = MathHelper.TwoPi * segmentIndex / segmentCount;
            var radialDirection = new Vector3(MathF.Cos(angleRadians), 0f, MathF.Sin(angleRadians));

            return radialDirection;
        }

        private static Vector3 FindGridNormal(IReadOnlyList<Vector3> gridPositions, int gridIndex, int rowLength, int rowCount)
        {
            int row = gridIndex / rowLength;
            int column = gridIndex % rowLength;
            Vector3 alongRow = gridPositions[(row * rowLength) + Math.Min(column + 1, rowLength - 1)] - gridPositions[(row * rowLength) + Math.Max(column - 1, 0)];
            Vector3 acrossRows = gridPositions[(Math.Min(row + 1, rowCount - 1) * rowLength) + column] - gridPositions[(Math.Max(row - 1, 0) * rowLength) + column];
            Vector3 gridNormal = Vector3.Cross(alongRow, acrossRows);
            if (gridNormal.Y < 0f)
            {
                gridNormal = -gridNormal;
            }

            gridNormal = gridNormal.LengthSquared() > 0f ? Vector3.Normalize(gridNormal) : Vector3.Up;

            return gridNormal;
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

        private void AddSphereOf(MeshPlacement meshPlacement, int segmentCount, int ringCount)
        {
            int firstVertex = _vertices.Count;
            int ringVertexCount = segmentCount + RingSeamVertexCount;
            for (var ringIndex = 0; ringIndex <= ringCount; ringIndex++)
            {
                AddSphereRing(ringIndex, segmentCount, ringCount, meshPlacement);
            }

            for (var ringIndex = 0; ringIndex < ringCount; ringIndex++)
            {
                AddSphereBand(firstVertex + (ringIndex * ringVertexCount), segmentCount);
            }
        }

        private void AddSphereRing(int ringIndex, int segmentCount, int ringCount, MeshPlacement meshPlacement)
        {
            float polarAngleRadians = MathHelper.Pi * ringIndex / ringCount;
            for (var segmentIndex = 0; segmentIndex <= segmentCount; segmentIndex++)
            {
                Vector3 radialDirection = CreateRadialDirection(segmentIndex, segmentCount);
                Vector3 spherePoint = (radialDirection * MathF.Sin(polarAngleRadians)) + (Vector3.Up * MathF.Cos(polarAngleRadians));
                AddVertex(new SurfacePoint(spherePoint, spherePoint), meshPlacement);
            }
        }

        private void AddSphereBand(int firstUpperVertexIndex, int segmentCount)
        {
            int ringVertexCount = segmentCount + RingSeamVertexCount;
            for (var segmentIndex = 0; segmentIndex < segmentCount; segmentIndex++)
            {
                int upperLeftIndex = firstUpperVertexIndex + segmentIndex;
                int lowerLeftIndex = upperLeftIndex + ringVertexCount;
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
