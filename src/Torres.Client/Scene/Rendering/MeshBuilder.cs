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

        // Unit box standing on Y = 0, like the cylinder, so a slab or a rim is placed by its base and not by its center.
        internal void AddBox(Matrix transform, Color color)
        {
            var placement = new Placement(transform, color);
            foreach (Vector3 faceNormal in _boxFaceNormals)
            {
                AddBoxFace(faceNormal, placement);
            }
        }

        internal void AddCylinder(Matrix transform, Color color)
        {
            var placement = new Placement(transform, color);
            int capCenter = AddVertex(new SurfacePoint(Vector3.Up, Vector3.Up), placement);
            int firstSegment = _vertices.Count;
            for (var segment = 0; segment <= CircleSegments; segment++)
            {
                Vector3 radialDirection = CreateRadialDirection(segment);
                Vector3 topPoint = radialDirection + Vector3.Up;
                AddVertex(new SurfacePoint(radialDirection, radialDirection), placement);
                AddVertex(new SurfacePoint(topPoint, radialDirection), placement);
                AddVertex(new SurfacePoint(topPoint, Vector3.Up), placement);
            }

            for (var segment = 0; segment < CircleSegments; segment++)
            {
                int bottom = firstSegment + (segment * CylinderVerticesPerSegment);
                int nextBottom = bottom + CylinderVerticesPerSegment;
                AddTriangle(bottom, nextBottom, nextBottom + CylinderSideTopOffset);
                AddTriangle(bottom, nextBottom + CylinderSideTopOffset, bottom + CylinderSideTopOffset);
                AddTriangle(capCenter, bottom + CylinderCapEdgeOffset, nextBottom + CylinderCapEdgeOffset);
            }
        }

        internal void AddSphere(Matrix transform, Color color)
        {
            var placement = new Placement(transform, color);
            int firstVertex = _vertices.Count;
            for (var ring = 0; ring <= SphereRings; ring++)
            {
                AddSphereRing(ring, placement);
            }

            for (var ring = 0; ring < SphereRings; ring++)
            {
                AddSphereBand(firstVertex + (ring * RingVertexCount));
            }
        }

        internal void AddDisc(Matrix transform, Color color)
        {
            var placement = new Placement(transform, color);
            int discCenter = AddVertex(new SurfacePoint(Vector3.Zero, Vector3.Up), placement);
            int firstEdge = _vertices.Count;
            for (var segment = 0; segment <= CircleSegments; segment++)
            {
                AddVertex(new SurfacePoint(CreateRadialDirection(segment), Vector3.Up), placement);
            }

            for (var segment = 0; segment < CircleSegments; segment++)
            {
                int edge = firstEdge + segment;
                AddTriangle(discCenter, edge, edge + 1);
            }
        }

        internal StaticMesh Build(GraphicsDevice graphicsDevice)
        {
            var mesh = new StaticMesh(graphicsDevice, _vertices.ToArray(), _indices.ToArray());

            return mesh;
        }

        private static Vector3 CreateRadialDirection(int segment)
        {
            float angleRadians = MathHelper.TwoPi * segment / CircleSegments;
            var radialDirection = new Vector3(MathF.Cos(angleRadians), 0f, MathF.Sin(angleRadians));

            return radialDirection;
        }

        private void AddBoxFace(Vector3 faceNormal, Placement placement)
        {
            var firstAxis = new Vector3(faceNormal.Y, faceNormal.Z, faceNormal.X);
            Vector3 secondAxis = Vector3.Cross(faceNormal, firstAxis);
            Vector3 faceCenter = _boxCenter + (faceNormal * HalfUnit);
            Vector3 diagonal = (firstAxis + secondAxis) * HalfUnit;
            Vector3 antidiagonal = (firstAxis - secondAxis) * HalfUnit;
            int firstCorner = AddVertex(new SurfacePoint(faceCenter - diagonal, faceNormal), placement);
            int secondCorner = AddVertex(new SurfacePoint(faceCenter + antidiagonal, faceNormal), placement);
            int thirdCorner = AddVertex(new SurfacePoint(faceCenter + diagonal, faceNormal), placement);
            int fourthCorner = AddVertex(new SurfacePoint(faceCenter - antidiagonal, faceNormal), placement);
            AddTriangle(firstCorner, secondCorner, thirdCorner);
            AddTriangle(firstCorner, thirdCorner, fourthCorner);
        }

        private void AddSphereRing(int ring, Placement placement)
        {
            float polarAngleRadians = MathHelper.Pi * ring / SphereRings;
            for (var segment = 0; segment <= CircleSegments; segment++)
            {
                Vector3 radialDirection = CreateRadialDirection(segment);
                Vector3 spherePoint = (radialDirection * MathF.Sin(polarAngleRadians)) + (Vector3.Up * MathF.Cos(polarAngleRadians));
                AddVertex(new SurfacePoint(spherePoint, spherePoint), placement);
            }
        }

        private void AddSphereBand(int firstUpperVertex)
        {
            for (var segment = 0; segment < CircleSegments; segment++)
            {
                int upperLeft = firstUpperVertex + segment;
                int lowerLeft = upperLeft + RingVertexCount;
                AddTriangle(upperLeft, lowerLeft, lowerLeft + 1);
                AddTriangle(upperLeft, lowerLeft + 1, upperLeft + 1);
            }
        }

        private int AddVertex(SurfacePoint localPoint, Placement placement)
        {
            Vector3 position = Vector3.Transform(localPoint.Position, placement.Transform);
            Vector3 normal = Vector3.Normalize(Vector3.TransformNormal(localPoint.Normal, placement.NormalTransform));
            _vertices.Add(new VertexPositionNormalColor(position, normal, placement.Color));
            int vertexIndex = _vertices.Count - 1;

            return vertexIndex;
        }

        // MonoGame culls counterclockwise triangles; the winding is decided here from the normal instead of in every shape.
        private void AddTriangle(int first, int second, int third)
        {
            Vector3 firstPosition = _vertices[first].Position;
            Vector3 winding = Vector3.Cross(_vertices[second].Position - firstPosition, _vertices[third].Position - firstPosition);
            Vector3 outward = _vertices[first].Normal + _vertices[second].Normal + _vertices[third].Normal;
            bool isClockwiseFromOutside = Vector3.Dot(winding, outward) < 0f;
            _indices.Add(first);
            if (isClockwiseFromOutside)
            {
                _indices.Add(second);
                _indices.Add(third);
            }
            else
            {
                _indices.Add(third);
                _indices.Add(second);
            }
        }
    }
}
