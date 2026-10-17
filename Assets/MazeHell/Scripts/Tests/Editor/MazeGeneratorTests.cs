#if UNITY_INCLUDE_TESTS
using System;
using System.Collections.Generic;
using System.Text;
using MazeHell.Level;
using NUnit.Framework;
using UnityEngine;

namespace MazeHell.Tests
{
    public class MazeGeneratorTests
    {
        private MazeGenerator _generator;

        [SetUp]
        public void SetUp()
        {
            _generator = new MazeGenerator();
        }

        [Test]
        public void GenerateConnectsEveryCell()
        {
            int[] sides = new int[] { 4, 5, 8, 13, 21 };
            int[] seeds = new int[] { 0, 1, 42, 1234567, -987654 };
            float[] loopFractions = new float[] { 0f, 0.25f, 0.5f, 1f };

            for (int sideIndex = 0; sideIndex < sides.Length; sideIndex++)
            {
                for (int seedIndex = 0; seedIndex < seeds.Length; seedIndex++)
                {
                    for (int fractionIndex = 0; fractionIndex < loopFractions.Length; fractionIndex++)
                    {
                        MazeLayout layout = _generator.Generate(sides[sideIndex], seeds[seedIndex], loopFractions[fractionIndex], 2);

                        Assert.IsTrue(layout.IsConnected(), $"Layout is not connected for side {sides[sideIndex]}, seed {seeds[seedIndex]}, loop fraction {loopFractions[fractionIndex]}.");
                    }
                }
            }
        }

        [Test]
        public void GenerateReachesExitFromStart()
        {
            MazeLayout layout = _generator.Generate(9, 777, 0.4f, 3);
            int exitDistance = layout.GetDistance(layout.StartCellIndex, layout.ExitCellIndex);

            Assert.Greater(exitDistance, 0, "Exit must be reachable from start and distinct from it.");
        }

        [Test]
        public void GenerateMakesExitFarthestFromStart()
        {
            MazeLayout layout = _generator.Generate(11, 2024, 0.6f, 2);
            int exitDistance = layout.GetDistance(layout.StartCellIndex, layout.ExitCellIndex);

            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                int cellDistance = layout.GetDistance(layout.StartCellIndex, cellIndex);

                Assert.LessOrEqual(cellDistance, exitDistance, $"Cell {cellIndex} is farther from start than exit.");
            }
        }

        [Test]
        public void GenerateSameSeedProducesIdenticalLayouts()
        {
            MazeLayout first = _generator.Generate(12, 31415, 0.7f, 4);
            MazeLayout second = new MazeGenerator().Generate(12, 31415, 0.7f, 4);

            Assert.AreEqual(BuildTopologySignature(first), BuildTopologySignature(second), "Same seed must reproduce the same layout.");
            Assert.AreEqual(first.StartCellIndex, second.StartCellIndex, "Same seed must reproduce the same start.");
            Assert.AreEqual(first.ExitCellIndex, second.ExitCellIndex, "Same seed must reproduce the same exit.");
        }

        [Test]
        public void GenerateDifferentSeedsProduceDifferentLayouts()
        {
            HashSet<string> signatures = new HashSet<string>();

            for (int seed = 0; seed < 8; seed++)
            {
                MazeLayout layout = _generator.Generate(10, seed, 0.5f, 2);
                signatures.Add(BuildTopologySignature(layout));
            }

            Assert.Greater(signatures.Count, 1, "Different seeds must produce different layouts.");
        }

        [Test]
        public void GenerateZeroLoopFractionProducesSpanningTree()
        {
            int[] sides = new int[] { 4, 7, 16 };

            for (int sideIndex = 0; sideIndex < sides.Length; sideIndex++)
            {
                MazeLayout layout = _generator.Generate(sides[sideIndex], 5, 0f, 0);
                int treePassageCount = 2 * (layout.Size - 1);

                Assert.AreEqual(treePassageCount, layout.CountPassages(), $"Loop fraction 0 must produce a tree for side {sides[sideIndex]}.");
            }
        }

        [Test]
        public void GenerateFullLoopFractionAddsCycles()
        {
            int[] sides = new int[] { 4, 8 };

            for (int sideIndex = 0; sideIndex < sides.Length; sideIndex++)
            {
                MazeLayout layout = _generator.Generate(sides[sideIndex], 9, 1f, 0);
                int treePassageCount = 2 * (layout.Size - 1);

                Assert.Greater(layout.CountPassages(), treePassageCount, $"Loop fraction 1 must add cycles for side {sides[sideIndex]}.");
            }
        }

        [Test]
        public void GenerateLoopFractionRaisesPassageCount()
        {
            MazeLayout tree = _generator.Generate(10, 3, 0f, 0);
            MazeLayout looped = _generator.Generate(10, 3, 1f, 0);

            Assert.Greater(looped.CountPassages(), tree.CountPassages(), "Higher loop fraction must open more passages.");
        }

        [Test]
        public void GenerateRetainsMinimumDeadEnds()
        {
            int[] sides = new int[] { 4, 9, 14 };
            float[] loopFractions = new float[] { 0.25f, 0.6f, 1f };

            for (int sideIndex = 0; sideIndex < sides.Length; sideIndex++)
            {
                for (int fractionIndex = 0; fractionIndex < loopFractions.Length; fractionIndex++)
                {
                    MazeLayout layout = _generator.Generate(sides[sideIndex], 17, loopFractions[fractionIndex], 2);

                    Assert.GreaterOrEqual(layout.CountDeadEnds(), 2, $"Minimum dead ends must survive for side {sides[sideIndex]}, loop fraction {loopFractions[fractionIndex]}.");
                }
            }
        }

        [Test]
        public void GenerateTreeAlwaysHasDeadEnds()
        {
            for (int seed = 0; seed < 6; seed++)
            {
                MazeLayout layout = _generator.Generate(10, seed, 0f, 0);

                Assert.GreaterOrEqual(layout.CountDeadEnds(), 2, $"Spanning tree must keep at least two dead ends for seed {seed}.");
            }
        }

        [Test]
        public void GenerateKeepsPassagesSymmetricAndCardinal()
        {
            MazeLayout layout = _generator.Generate(9, 64, 0.8f, 2);

            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                MazeCell cell = layout.Cells[cellIndex];

                for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
                {
                    MazeDirection direction = (MazeDirection)directionIndex;
                    MazeCell neighbor;
                    bool neighborFound = layout.TryGetNeighbor(cell, direction, out neighbor);

                    Assert.AreEqual(cell.HasPassage(direction), neighborFound, $"Passage flags must match neighbors for cell {cellIndex}.");

                    if (neighborFound)
                    {
                        Vector2Int offset = MazeDirections.Delta(direction);

                        Assert.AreEqual(cell.Coordinates + offset, neighbor.Coordinates, $"Neighbors must be cardinal for cell {cellIndex}.");
                    }
                }
            }
        }

        [Test]
        public void GenerateKeepsBoundaryCellsClosed()
        {
            MazeLayout layout = _generator.Generate(8, 3, 0.9f, 0);

            for (int coordinate = 0; coordinate < layout.Side; coordinate++)
            {
                MazeCell southCell = layout.Cells[coordinate];
                MazeCell northCell = layout.Cells[(layout.Side - 1) * layout.Side + coordinate];
                MazeCell westCell = layout.Cells[coordinate * layout.Side];
                MazeCell eastCell = layout.Cells[coordinate * layout.Side + layout.Side - 1];

                Assert.IsFalse(southCell.HasPassage(MazeDirection.South), "South border must stay closed.");
                Assert.IsFalse(northCell.HasPassage(MazeDirection.North), "North border must stay closed.");
                Assert.IsFalse(westCell.HasPassage(MazeDirection.West), "West border must stay closed.");
                Assert.IsFalse(eastCell.HasPassage(MazeDirection.East), "East border must stay closed.");
            }
        }

        [Test]
        public void GenerateMatchesCoordinatesWithIndexes()
        {
            MazeLayout layout = _generator.Generate(7, 11, 0.5f, 1);

            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                MazeCell cell = layout.Cells[cellIndex];
                int expectedIndex = cell.Coordinates.y * layout.Side + cell.Coordinates.x;

                Assert.AreEqual(expectedIndex, cell.Index, "Coordinates must match the cell index.");
            }
        }

        [Test]
        public void GenerateNeighborsMatchPassageCount()
        {
            MazeLayout layout = _generator.Generate(9, 88, 0.5f, 2);

            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                MazeCell cell = layout.Cells[cellIndex];

                Assert.AreEqual(cell.PassageCount, layout.GetNeighbors(cell).Count, $"Neighbor list must match passage count for cell {cellIndex}.");
            }
        }

        [Test]
        public void GenerateRejectsInvalidArguments()
        {
            Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(3, 1, 0.5f, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(4, 1, -0.01f, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(4, 1, 1.01f, 0));
            Assert.Throws<ArgumentOutOfRangeException>(() => _generator.Generate(4, 1, 0.5f, -1));
        }

        private string BuildTopologySignature(MazeLayout layout)
        {
            StringBuilder builder = new StringBuilder();
            builder.Append(layout.Side);

            for (int cellIndex = 0; cellIndex < layout.Size; cellIndex++)
            {
                MazeCell cell = layout.Cells[cellIndex];

                for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
                {
                    builder.Append(Convert.ToInt32(cell.HasPassage((MazeDirection)directionIndex)));
                }
            }

            return builder.ToString();
        }
    }
}
#endif
