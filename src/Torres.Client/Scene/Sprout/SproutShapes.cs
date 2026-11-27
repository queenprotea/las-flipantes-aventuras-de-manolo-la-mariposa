using System;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class SproutShapes
    {
        internal const float StemPlaneDepth = 6f;

        private const int StemSegments = 32;
        private const float StemBaseRadius = 0.1f;
        private const float StemTipRadius = 0.035f;
        private const float StemTaperPower = 0.9f;
        private const int HeightSearchSteps = 40;
        private const float CurveWeight = 3f;
        private const float QuadraticMiddleWeight = 2f;
        private const float BisectionDivisor = 2f;
        private const int StemSecondInnerControl = 2;
        private const int StemEndControl = 3;
        private const int PedicelEndControl = 2;
        private const float FullParameter = 1f;
        private const float StraightDotLimit = 0.9999f;
        private const int PedicelSegments = 8;
        private const float PedicelRadius = 0.026f;
        private const float BudCenterReach = 0.14f;
        private const float BudTipReach = 0.27f;
        private const float SepalReach = 0.06f;
        private const float SepalSpread = 0.075f;
        private const int SepalCount = 4;
        private const float SepalPhaseDegrees = 20f;
        private const float SepalTiltDegrees = 28f;
        private const float NodeRadiusFactor = 1.25f;

        private static readonly Vector2[] _stemControls = new Vector2[]
        {
            new Vector2(-8.2f, -2.8f), new Vector2(-7.3f, 0.6f), new Vector2(-6.5f, 3.0f), new Vector2(-5.3f, 3.9f),
        };

        private static readonly Vector3[] _pedicelControls = new Vector3[]
        {
            Vector3.Zero, new Vector3(0.28f, 0.14f, 0f), new Vector3(0.5f, -0.06f, 0f),
        };

        private static readonly Vector3 _budScale = new Vector3(0.11f, 0.17f, 0.11f);
        private static readonly Vector3 _budTipScale = new Vector3(0.045f, 0.06f, 0.045f);
        private static readonly Vector3 _sepalScale = new Vector3(0.065f, 0.1f, 0.026f);

        internal static Vector3 StemTip { get; } = FindStemPoint(FullParameter);

        internal static Vector3 FindStemPointAtHeight(float height)
        {
            Vector3 stemPoint = FindStemPoint(FindStemParameterAtHeight(height));

            return stemPoint;
        }

        internal static float FindStemRadiusAtHeight(float height)
        {
            float stemRadius = FindStemRadius(FindStemParameterAtHeight(height));

            return stemRadius;
        }

        internal static void AddStem(MeshBuilder meshBuilder)
        {
            for (var segmentIndex = 0; segmentIndex < StemSegments; segmentIndex++)
            {
                float startParameter = (float)segmentIndex / StemSegments;
                float endParameter = (float)(segmentIndex + 1) / StemSegments;
                float segmentRadius = FindStemRadius(startParameter);
                Color segmentColor = Color.Lerp(GardenPalette.SproutStemBase, GardenPalette.SproutStemTip, startParameter);
                meshBuilder.AddCylinder(CreateTubeTransform(FindStemPoint(startParameter), FindStemPoint(endParameter), segmentRadius), segmentColor);
                meshBuilder.AddSphere(Matrix.CreateScale(segmentRadius) * Matrix.CreateTranslation(FindStemPoint(endParameter)), segmentColor);
            }
        }

        internal static void AddNode(MeshBuilder meshBuilder, Vector3 nodeCenter, float stemRadius)
        {
            meshBuilder.AddSphere(Matrix.CreateScale(stemRadius * NodeRadiusFactor) * Matrix.CreateTranslation(nodeCenter), GardenPalette.SproutStemTip);
        }

        internal static void AddBud(MeshBuilder meshBuilder)
        {
            for (var segmentIndex = 0; segmentIndex < PedicelSegments; segmentIndex++)
            {
                Vector3 segmentStart = FindPedicelPoint((float)segmentIndex / PedicelSegments);
                Vector3 segmentEnd = FindPedicelPoint((float)(segmentIndex + 1) / PedicelSegments);
                meshBuilder.AddCylinder(CreateTubeTransform(segmentStart, segmentEnd, PedicelRadius), GardenPalette.SproutStemTip);
                meshBuilder.AddSphere(Matrix.CreateScale(PedicelRadius) * Matrix.CreateTranslation(segmentEnd), GardenPalette.SproutStemTip);
            }

            Vector3 pedicelEnd = FindPedicelPoint(FullParameter);
            Vector3 budAxis = Vector3.Normalize(pedicelEnd - FindPedicelPoint(FullParameter - (FullParameter / PedicelSegments)));
            Matrix budRotation = CreateRotationFromUp(budAxis);
            meshBuilder.AddSphere(Matrix.CreateScale(_budScale) * budRotation * Matrix.CreateTranslation(pedicelEnd + (budAxis * BudCenterReach)), GardenPalette.SproutBud);
            meshBuilder.AddSphere(Matrix.CreateScale(_budTipScale) * budRotation * Matrix.CreateTranslation(pedicelEnd + (budAxis * BudTipReach)), GardenPalette.SproutBudTip);
            for (var sepalIndex = 0; sepalIndex < SepalCount; sepalIndex++)
            {
                float headingDegrees = FlowerParts.GetCircleHeadingDegrees(sepalIndex, SepalCount, SepalPhaseDegrees);
                Matrix sepalPlacement = Matrix.CreateScale(_sepalScale)
                    * Matrix.CreateRotationX(MathHelper.ToRadians(SepalTiltDegrees))
                    * Matrix.CreateTranslation(0f, SepalReach, SepalSpread)
                    * Matrix.CreateRotationY(MathHelper.ToRadians(headingDegrees))
                    * budRotation
                    * Matrix.CreateTranslation(pedicelEnd);
                meshBuilder.AddSphere(sepalPlacement, GardenPalette.SproutSepal);
            }
        }

        private static Vector3 FindStemPoint(float stemParameter)
        {
            float remainingParameter = FullParameter - stemParameter;
            Vector2 curvePoint = (_stemControls[0] * remainingParameter * remainingParameter * remainingParameter)
                + (_stemControls[1] * CurveWeight * stemParameter * remainingParameter * remainingParameter)
                + (_stemControls[StemSecondInnerControl] * CurveWeight * stemParameter * stemParameter * remainingParameter)
                + (_stemControls[StemEndControl] * stemParameter * stemParameter * stemParameter);
            var stemPoint = new Vector3(curvePoint.X, curvePoint.Y, StemPlaneDepth);

            return stemPoint;
        }

        private static float FindStemParameterAtHeight(float height)
        {
            float lowParameter = 0f;
            float highParameter = FullParameter;
            for (var searchStep = 0; searchStep < HeightSearchSteps; searchStep++)
            {
                float middleParameter = (lowParameter + highParameter) / BisectionDivisor;
                if (FindStemPoint(middleParameter).Y < height)
                {
                    lowParameter = middleParameter;
                }
                else
                {
                    highParameter = middleParameter;
                }
            }

            return lowParameter;
        }

        private static float FindStemRadius(float stemParameter)
        {
            float stemRadius = MathHelper.Lerp(StemBaseRadius, StemTipRadius, MathF.Pow(stemParameter, StemTaperPower));

            return stemRadius;
        }

        private static Vector3 FindPedicelPoint(float pedicelParameter)
        {
            float remainingParameter = FullParameter - pedicelParameter;
            Vector3 pedicelPoint = (_pedicelControls[0] * remainingParameter * remainingParameter)
                + (_pedicelControls[1] * QuadraticMiddleWeight * pedicelParameter * remainingParameter)
                + (_pedicelControls[PedicelEndControl] * pedicelParameter * pedicelParameter);

            return pedicelPoint;
        }

        private static Matrix CreateTubeTransform(Vector3 tubeStart, Vector3 tubeEnd, float tubeRadius)
        {
            Vector3 tubeLine = tubeEnd - tubeStart;
            float tubeLength = tubeLine.Length();
            Matrix tubeTransform = Matrix.CreateScale(tubeRadius, tubeLength, tubeRadius) * CreateRotationFromUp(tubeLine / tubeLength) * Matrix.CreateTranslation(tubeStart);

            return tubeTransform;
        }

        private static Matrix CreateRotationFromUp(Vector3 direction)
        {
            float upDot = Vector3.Dot(Vector3.Up, direction);
            Matrix rotation = Matrix.Identity;
            if (Math.Abs(upDot) < StraightDotLimit)
            {
                Vector3 rotationAxis = Vector3.Normalize(Vector3.Cross(Vector3.Up, direction));
                rotation = Matrix.CreateFromAxisAngle(rotationAxis, MathF.Acos(upDot));
            }

            return rotation;
        }
    }
}
