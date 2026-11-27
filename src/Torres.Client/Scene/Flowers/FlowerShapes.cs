using System;
using System.Collections.Generic;
using System.Linq;

using Microsoft.Xna.Framework;

namespace Torres.Client.Scene
{
    internal static class FlowerShapes
    {
        private static readonly IReadOnlyList<FlowerSpecies> _speciesByPlanter = new FlowerSpecies[]
        {
            FlowerSpecies.Allium, FlowerSpecies.Lavender, FlowerSpecies.Protea, FlowerSpecies.Coneflower, FlowerSpecies.Narcissus,
        };

        internal static void AddFlowers(MeshBuilder meshBuilder, BoardLayout layout, IReadOnlyCollection<BoardCell> skippedCells)
        {
            foreach (BoardCell drawnCell in BoardLayout.EnumerateCells())
            {
                int flowerLevel = layout.GetFlowerLevel(drawnCell);
                if ((flowerLevel > 0) && !skippedCells.Contains(drawnCell))
                {
                    AddFlower(meshBuilder, BoardGeometry.GetCellCenter(drawnCell), new PlantedFlower(flowerLevel, FindSpecies(layout.GetPlanterNumber(drawnCell))));
                }
            }
        }

        internal static void AddCenteredFlower(MeshBuilder meshBuilder, BoardLayout layout, BoardCell flowerCell)
        {
            AddFlower(meshBuilder, Vector3.Zero, new PlantedFlower(layout.GetFlowerLevel(flowerCell), FindSpecies(layout.GetPlanterNumber(flowerCell))));
        }

        private static FlowerSpecies FindSpecies(int planterNumber)
        {
            int speciesIndex = (planterNumber - BoardLayout.FirstPlanterNumber) % _speciesByPlanter.Count;
            FlowerSpecies flowerSpecies = _speciesByPlanter[speciesIndex];

            return flowerSpecies;
        }

        private static void AddFlower(MeshBuilder meshBuilder, Vector3 cellCenter, PlantedFlower plantedFlower)
        {
            switch (plantedFlower.Species)
            {
                case FlowerSpecies.Allium:
                    AlliumShapes.AddFlower(meshBuilder, cellCenter, plantedFlower.Level);
                    break;
                case FlowerSpecies.Lavender:
                    LavenderShapes.AddFlower(meshBuilder, cellCenter, plantedFlower.Level);
                    break;
                case FlowerSpecies.Protea:
                    ProteaShapes.AddFlower(meshBuilder, cellCenter, plantedFlower.Level);
                    break;
                case FlowerSpecies.Coneflower:
                    ConeflowerShapes.AddFlower(meshBuilder, cellCenter, plantedFlower.Level);
                    break;
                case FlowerSpecies.Narcissus:
                    NarcissusShapes.AddFlower(meshBuilder, cellCenter, plantedFlower.Level);
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(plantedFlower));
            }
        }
    }
}
