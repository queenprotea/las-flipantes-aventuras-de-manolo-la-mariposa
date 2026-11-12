using System;
using System.Collections.Generic;
using System.Linq;

using Game.Contracts;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Torres.Client.Ui;

namespace Torres.Client.Scene
{
    internal sealed class CaterpillarFigure : IDisposable
    {
        private const double WavePeriodSeconds = 1.6;
        private const float CellPhaseStepRadians = 0.9f;
        private const double WalkSeconds = 0.45;
        private const double DropSeconds = 0.4;
        private const double VanishSeconds = 0.4;
        private const float WalkHopHeight = 0.08f;
        private const float ClimbHopShare = 0.6f;
        private const float DropHeight = 0.9f;
        private const float FullScale = 1f;
        private const int SingleCaterpillarCount = 1;
        private const int NeighborDistance = 1;

        private readonly GraphicsDevice _graphicsDevice;
        private readonly StaticMesh _sphereMesh;
        private readonly StaticMesh _cylinderMesh;
        private readonly List<CaterpillarPart> _parts = new List<CaterpillarPart>();
        private readonly List<CaterpillarMotion> _motions = new List<CaterpillarMotion>();

        internal CaterpillarFigure(GraphicsDevice graphicsDevice)
        {
            _graphicsDevice = graphicsDevice;

            var sphereBuilder = new MeshBuilder();
            sphereBuilder.AddSphere(Matrix.Identity, Color.White);
            _sphereMesh = sphereBuilder.BuildMesh(graphicsDevice);

            var cylinderBuilder = new MeshBuilder();
            cylinderBuilder.AddCylinder(Matrix.Identity, Color.White);
            _cylinderMesh = cylinderBuilder.BuildMesh(graphicsDevice);
        }

        internal void PlanMotions(LayoutChange layoutChange, TimeSpan sceneTime)
        {
            _motions.Clear();
            IEnumerable<BoardCaterpillar> involvedCaterpillars = layoutChange.PreviousLayout.Caterpillars.Concat(layoutChange.NextLayout.Caterpillars);
            foreach (SeatColor seatColor in involvedCaterpillars.Select(caterpillar => caterpillar.SeatColor).Distinct())
            {
                PlanSeatMotions(layoutChange, seatColor, sceneTime);
            }
        }

        internal void ClearMotions()
        {
            _motions.Clear();
        }

        internal void Draw(BasicEffect sceneEffect, BoardLayout layout, TimeSpan sceneTime)
        {
            float wavePhaseRadians = SceneTiming.FindCycleRadians(sceneTime, WavePeriodSeconds);
            foreach (BoardCaterpillar caterpillar in layout.Caterpillars)
            {
                Vector3 restingOrigin = FindBodyOrigin(layout, caterpillar);
                CaterpillarMotion? arrivalMotion = FindArrivalMotion(caterpillar);
                CaterpillarMotionSample motionSample = arrivalMotion is CaterpillarMotion motion ? FindMotionSample(motion, sceneTime) : new CaterpillarMotionSample(restingOrigin, FullScale);
                DrawPose(sceneEffect, CreatePose(caterpillar, motionSample, wavePhaseRadians));
            }

            foreach (CaterpillarMotion vanishMotion in _motions.Where(motion => motion.Kind == CaterpillarMotionKind.Vanish))
            {
                CaterpillarMotionSample motionSample = FindMotionSample(vanishMotion, sceneTime);
                if (motionSample.Scale > 0f)
                {
                    DrawPose(sceneEffect, CreatePose(vanishMotion.Caterpillar, motionSample, wavePhaseRadians));
                }
            }

            sceneEffect.DiffuseColor = Vector3.One;
            sceneEffect.World = Matrix.Identity;
        }

        public void Dispose()
        {
            _sphereMesh.Dispose();
            _cylinderMesh.Dispose();
        }

        private static Vector3 FindBodyOrigin(BoardLayout layout, BoardCaterpillar caterpillar)
        {
            Vector3 bodyOrigin = BoardGeometry.GetCellCenter(caterpillar.Cell) + new Vector3(0f, CaterpillarShapes.FindBaseHeight(layout, caterpillar.Cell), 0f);

            return bodyOrigin;
        }

        private static bool AreNeighbors(BoardCell firstCell, BoardCell secondCell)
        {
            int cellDistance = Math.Abs(firstCell.Row - secondCell.Row) + Math.Abs(firstCell.Column - secondCell.Column);
            bool areNeighbors = cellDistance == NeighborDistance;

            return areNeighbors;
        }

        private static CaterpillarPose CreatePose(BoardCaterpillar caterpillar, CaterpillarMotionSample motionSample, float wavePhaseRadians)
        {
            float cellPhaseRadians = CellPhaseStepRadians * ((caterpillar.Cell.Row * BoardLayout.CellsPerSide) + caterpillar.Cell.Column);
            var pose = new CaterpillarPose(motionSample.BodyOrigin, caterpillar.HeadingDegrees, SeatColors.ColorFor(caterpillar.SeatColor), wavePhaseRadians + cellPhaseRadians, motionSample.Scale);

            return pose;
        }

        private static CaterpillarMotionSample FindMotionSample(CaterpillarMotion motion, TimeSpan sceneTime)
        {
            CaterpillarMotionSample motionSample = motion.Kind switch
            {
                CaterpillarMotionKind.Walk => FindWalkSample(motion, sceneTime),
                CaterpillarMotionKind.Drop => FindDropSample(motion, sceneTime),
                CaterpillarMotionKind.Vanish => FindVanishSample(motion, sceneTime),
                _ => throw new ArgumentOutOfRangeException(nameof(motion)),
            };

            return motionSample;
        }

        private static CaterpillarMotionSample FindWalkSample(CaterpillarMotion motion, TimeSpan sceneTime)
        {
            float easedShare = MathHelper.SmoothStep(0f, 1f, FindMotionShare(motion, sceneTime, WalkSeconds));
            float hopHeight = WalkHopHeight + (ClimbHopShare * Math.Max(0f, motion.EndBodyOrigin.Y - motion.StartBodyOrigin.Y));
            float hopLift = SceneTiming.FindArcLift(hopHeight, easedShare);
            Vector3 walkOrigin = Vector3.Lerp(motion.StartBodyOrigin, motion.EndBodyOrigin, easedShare) + new Vector3(0f, hopLift, 0f);
            var motionSample = new CaterpillarMotionSample(walkOrigin, FullScale);

            return motionSample;
        }

        private static CaterpillarMotionSample FindDropSample(CaterpillarMotion motion, TimeSpan sceneTime)
        {
            float dropShare = FindMotionShare(motion, sceneTime, DropSeconds);
            float fallLift = DropHeight * (1f - (dropShare * dropShare));
            var motionSample = new CaterpillarMotionSample(motion.EndBodyOrigin + new Vector3(0f, fallLift, 0f), FullScale);

            return motionSample;
        }

        private static CaterpillarMotionSample FindVanishSample(CaterpillarMotion motion, TimeSpan sceneTime)
        {
            float vanishScale = FullScale - MathHelper.SmoothStep(0f, 1f, FindMotionShare(motion, sceneTime, VanishSeconds));
            var motionSample = new CaterpillarMotionSample(motion.StartBodyOrigin, vanishScale);

            return motionSample;
        }

        private static float FindMotionShare(CaterpillarMotion motion, TimeSpan sceneTime, double motionSeconds)
        {
            double elapsedSeconds = (sceneTime - motion.StartTime).TotalSeconds;
            float motionShare = (float)Math.Clamp(elapsedSeconds / motionSeconds, 0d, 1d);

            return motionShare;
        }

        private static List<BoardCaterpillar> FindCaterpillarsMissingFrom(BoardLayout presentLayout, BoardLayout otherLayout, SeatColor seatColor)
        {
            List<BoardCaterpillar> missingCaterpillars = presentLayout.Caterpillars.Where(caterpillar => (caterpillar.SeatColor == seatColor) && !otherLayout.Caterpillars.Contains(caterpillar)).ToList();

            return missingCaterpillars;
        }

        private CaterpillarMotion? FindArrivalMotion(BoardCaterpillar caterpillar)
        {
            CaterpillarMotion? arrivalMotion = null;
            foreach (CaterpillarMotion motion in _motions)
            {
                if ((motion.Kind != CaterpillarMotionKind.Vanish) && (motion.Caterpillar == caterpillar))
                {
                    arrivalMotion = motion;
                }
            }

            return arrivalMotion;
        }

        private void PlanSeatMotions(LayoutChange layoutChange, SeatColor seatColor, TimeSpan sceneTime)
        {
            List<BoardCaterpillar> leavingCaterpillars = FindCaterpillarsMissingFrom(layoutChange.PreviousLayout, layoutChange.NextLayout, seatColor);
            List<BoardCaterpillar> arrivingCaterpillars = FindCaterpillarsMissingFrom(layoutChange.NextLayout, layoutChange.PreviousLayout, seatColor);
            bool isWalk = (leavingCaterpillars.Count == SingleCaterpillarCount)
                && (arrivingCaterpillars.Count == SingleCaterpillarCount)
                && AreNeighbors(leavingCaterpillars[0].Cell, arrivingCaterpillars[0].Cell);
            if (isWalk)
            {
                Vector3 startBodyOrigin = FindBodyOrigin(layoutChange.PreviousLayout, leavingCaterpillars[0]);
                Vector3 endBodyOrigin = FindBodyOrigin(layoutChange.NextLayout, arrivingCaterpillars[0]);
                _motions.Add(new CaterpillarMotion(CaterpillarMotionKind.Walk, arrivingCaterpillars[0], startBodyOrigin, endBodyOrigin, sceneTime));
            }
            else if (leavingCaterpillars.Count == 0)
            {
                foreach (BoardCaterpillar arrivingCaterpillar in arrivingCaterpillars)
                {
                    Vector3 dropBodyOrigin = FindBodyOrigin(layoutChange.NextLayout, arrivingCaterpillar);
                    _motions.Add(new CaterpillarMotion(CaterpillarMotionKind.Drop, arrivingCaterpillar, dropBodyOrigin, dropBodyOrigin, sceneTime));
                }
            }
            else if (arrivingCaterpillars.Count == 0)
            {
                foreach (BoardCaterpillar leavingCaterpillar in leavingCaterpillars)
                {
                    Vector3 vanishBodyOrigin = FindBodyOrigin(layoutChange.PreviousLayout, leavingCaterpillar);
                    _motions.Add(new CaterpillarMotion(CaterpillarMotionKind.Vanish, leavingCaterpillar, vanishBodyOrigin, vanishBodyOrigin, sceneTime));
                }
            }
        }

        private void DrawPose(BasicEffect sceneEffect, CaterpillarPose pose)
        {
            _parts.Clear();
            CaterpillarShapes.AddParts(_parts, pose);
            Matrix poseScaling = Matrix.CreateTranslation(-pose.BodyOrigin) * Matrix.CreateScale(pose.Scale) * Matrix.CreateTranslation(pose.BodyOrigin);
            foreach (CaterpillarPart part in _parts)
            {
                sceneEffect.World = part.Transform * poseScaling;
                sceneEffect.DiffuseColor = part.Color.ToVector3();
                StaticMesh partMesh = part.Shape == CaterpillarPartShape.Cylinder ? _cylinderMesh : _sphereMesh;
                partMesh.Draw(_graphicsDevice, sceneEffect);
            }
        }
    }
}
