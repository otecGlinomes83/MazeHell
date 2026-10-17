using System;
using System.Collections.Generic;
using UnityEngine;
using Random = System.Random;

namespace MazeHell.Level
{
    public class MazeGenerator
    {
        private const int NoParentCellIndex = -1;
        private const int UnreachedDistance = -1;

        public MazeLayout Generate(int side, int seed, float loopFraction, int minimumDeadEnds)
        {
            ValidateArguments(side, loopFraction, minimumDeadEnds);

            int cellCount = side * side;
            System.Random random = new System.Random(seed);

            bool[] northOpen = new bool[cellCount];
            bool[] eastOpen = new bool[cellCount];
            bool[] southOpen = new bool[cellCount];
            bool[] westOpen = new bool[cellCount];

            CarveSpanningTree(side, random, northOpen, eastOpen, southOpen, westOpen);
            AddLoopPassages(side, random, loopFraction, minimumDeadEnds, northOpen, eastOpen, southOpen, westOpen);

            MazeCell[] cells = BuildCells(side, northOpen, eastOpen, southOpen, westOpen);
            int startCellIndex = FindFarthestCellIndex(cells, side, 0);
            int exitCellIndex = FindFarthestCellIndex(cells, side, startCellIndex);

            return new MazeLayout(side, seed, startCellIndex, exitCellIndex, cells);
        }

        private void ValidateArguments(int side, float loopFraction, int minimumDeadEnds)
        {
            if (side < MazeLayout.MinimumSide)
            {
                throw new ArgumentOutOfRangeException(nameof(side), side, $"Side must be at least {MazeLayout.MinimumSide}.");
            }

            if (loopFraction < 0f || loopFraction > 1f)
            {
                throw new ArgumentOutOfRangeException(nameof(loopFraction), loopFraction, "Loop fraction must be within [0, 1].");
            }

            if (minimumDeadEnds < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(minimumDeadEnds), minimumDeadEnds, "Minimum dead ends must not be negative.");
            }
        }

        private void CarveSpanningTree(int side, System.Random random, bool[] northOpen, bool[] eastOpen, bool[] southOpen, bool[] westOpen)
        {
            int cellCount = side * side;
            bool[] visited = new bool[cellCount];
            Stack<SpanningTreeStep> pendingSteps = new Stack<SpanningTreeStep>();

            visited[0] = true;
            pendingSteps.Push(new SpanningTreeStep(0, NoParentCellIndex, MazeDirection.North));

            while (pendingSteps.Count > 0)
            {
                SpanningTreeStep step = pendingSteps.Pop();

                if (step.ParentCellIndex >= 0)
                {
                    OpenCorridor(step.CellIndex, step.ParentCellIndex, step.DirectionToParent, northOpen, eastOpen, southOpen, westOpen);
                }

                List<MazeDirection> moves = CollectUnvisitedMoves(side, step.CellIndex, visited);
                Shuffle(moves, random);

                for (int moveIndex = 0; moveIndex < moves.Count; moveIndex++)
                {
                    MazeDirection moveDirection = moves[moveIndex];
                    int neighborIndex = IndexOfNeighbor(side, step.CellIndex, moveDirection);

                    visited[neighborIndex] = true;
                    pendingSteps.Push(new SpanningTreeStep(neighborIndex, step.CellIndex, MazeDirections.Opposite(moveDirection)));
                }
            }
        }

        private List<MazeDirection> CollectUnvisitedMoves(int side, int cellIndex, bool[] visited)
        {
            List<MazeDirection> moves = new List<MazeDirection>(MazeDirections.Count);
            int cellCoordinateX = cellIndex % side;
            int cellCoordinateY = cellIndex / side;

            for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
            {
                MazeDirection direction = (MazeDirection)directionIndex;
                Vector2Int delta = MazeDirections.Delta(direction);
                int neighborCoordinateX = cellCoordinateX + delta.x;
                int neighborCoordinateY = cellCoordinateY + delta.y;

                if (neighborCoordinateX < 0 || neighborCoordinateX >= side || neighborCoordinateY < 0 || neighborCoordinateY >= side)
                {
                    continue;
                }

                int neighborIndex = neighborCoordinateY * side + neighborCoordinateX;

                if (visited[neighborIndex])
                {
                    continue;
                }

                moves.Add(direction);
            }

            return moves;
        }

        private void AddLoopPassages(int side, System.Random random, float loopFraction, int minimumDeadEnds, bool[] northOpen, bool[] eastOpen, bool[] southOpen, bool[] westOpen)
        {
            int[] passageCounts = BuildPassageCounts(northOpen, eastOpen, southOpen, westOpen);
            int deadEndCount = CountDeadEnds(passageCounts);

            List<LoopCandidate> candidates = CollectLoopCandidates(side, northOpen, eastOpen, southOpen, westOpen);
            Shuffle(candidates, random);

            int targetLoopCount = (int)Math.Round((double)loopFraction * candidates.Count);
            int addedLoopCount = 0;

            for (int candidateIndex = 0; candidateIndex < candidates.Count && addedLoopCount < targetLoopCount; candidateIndex++)
            {
                LoopCandidate candidate = candidates[candidateIndex];
                int neighborIndex = IndexOfNeighbor(side, candidate.CellIndex, candidate.Direction);
                int projectedDeadEndCount = deadEndCount - CountDeadEndsAmong(passageCounts, candidate.CellIndex, neighborIndex);

                if (projectedDeadEndCount < minimumDeadEnds)
                {
                    continue;
                }

                OpenCorridor(candidate.CellIndex, neighborIndex, candidate.Direction, northOpen, eastOpen, southOpen, westOpen);
                passageCounts[candidate.CellIndex] += 1;
                passageCounts[neighborIndex] += 1;
                deadEndCount = projectedDeadEndCount;
                addedLoopCount += 1;
            }
        }

        private List<LoopCandidate> CollectLoopCandidates(int side, bool[] northOpen, bool[] eastOpen, bool[] southOpen, bool[] westOpen)
        {
            List<LoopCandidate> candidates = new List<LoopCandidate>();

            for (int cellCoordinateY = 0; cellCoordinateY < side; cellCoordinateY++)
            {
                for (int cellCoordinateX = 0; cellCoordinateX < side; cellCoordinateX++)
                {
                    int cellIndex = cellCoordinateY * side + cellCoordinateX;

                    if (cellCoordinateX < side - 1 && eastOpen[cellIndex] == false)
                    {
                        candidates.Add(new LoopCandidate(cellIndex, MazeDirection.East));
                    }

                    if (cellCoordinateY < side - 1 && northOpen[cellIndex] == false)
                    {
                        candidates.Add(new LoopCandidate(cellIndex, MazeDirection.North));
                    }
                }
            }

            return candidates;
        }

        private int[] BuildPassageCounts(bool[] northOpen, bool[] eastOpen, bool[] southOpen, bool[] westOpen)
        {
            int cellCount = northOpen.Length;
            int[] passageCounts = new int[cellCount];

            for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
            {
                passageCounts[cellIndex] = Convert.ToInt32(northOpen[cellIndex]) + Convert.ToInt32(eastOpen[cellIndex]) + Convert.ToInt32(southOpen[cellIndex]) + Convert.ToInt32(westOpen[cellIndex]);
            }

            return passageCounts;
        }

        private int CountDeadEnds(int[] passageCounts)
        {
            int deadEndCount = 0;

            for (int cellIndex = 0; cellIndex < passageCounts.Length; cellIndex++)
            {
                if (passageCounts[cellIndex] == 1)
                {
                    deadEndCount += 1;
                }
            }

            return deadEndCount;
        }

        private int CountDeadEndsAmong(int[] passageCounts, int firstCellIndex, int secondCellIndex)
        {
            int deadEndCount = 0;

            if (passageCounts[firstCellIndex] == 1)
            {
                deadEndCount += 1;
            }

            if (passageCounts[secondCellIndex] == 1)
            {
                deadEndCount += 1;
            }

            return deadEndCount;
        }

        private void OpenCorridor(int firstCellIndex, int secondCellIndex, MazeDirection directionFromFirstToSecond, bool[] northOpen, bool[] eastOpen, bool[] southOpen, bool[] westOpen)
        {
            OpenPassage(firstCellIndex, directionFromFirstToSecond, northOpen, eastOpen, southOpen, westOpen);
            OpenPassage(secondCellIndex, MazeDirections.Opposite(directionFromFirstToSecond), northOpen, eastOpen, southOpen, westOpen);
        }

        private void OpenPassage(int cellIndex, MazeDirection direction, bool[] northOpen, bool[] eastOpen, bool[] southOpen, bool[] westOpen)
        {
            switch (direction)
            {
                case MazeDirection.North:
                    northOpen[cellIndex] = true;
                    break;
                case MazeDirection.East:
                    eastOpen[cellIndex] = true;
                    break;
                case MazeDirection.South:
                    southOpen[cellIndex] = true;
                    break;
                case MazeDirection.West:
                    westOpen[cellIndex] = true;
                    break;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Unknown maze direction {direction}.");
            }
        }

        private MazeCell[] BuildCells(int side, bool[] northOpen, bool[] eastOpen, bool[] southOpen, bool[] westOpen)
        {
            int cellCount = side * side;
            MazeCell[] cells = new MazeCell[cellCount];

            for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
            {
                Vector2Int coordinates = new Vector2Int(cellIndex % side, cellIndex / side);
                cells[cellIndex] = new MazeCell(cellIndex, coordinates, northOpen[cellIndex], eastOpen[cellIndex], southOpen[cellIndex], westOpen[cellIndex]);
            }

            return cells;
        }

        private int FindFarthestCellIndex(MazeCell[] cells, int side, int fromCellIndex)
        {
            int cellCount = cells.Length;
            int[] distances = new int[cellCount];

            for (int cellIndex = 0; cellIndex < cellCount; cellIndex++)
            {
                distances[cellIndex] = UnreachedDistance;
            }

            Queue<int> pendingCells = new Queue<int>();
            distances[fromCellIndex] = 0;
            pendingCells.Enqueue(fromCellIndex);

            int farthestCellIndex = fromCellIndex;
            int farthestDistance = 0;

            while (pendingCells.Count > 0)
            {
                int currentCellIndex = pendingCells.Dequeue();
                MazeCell currentCell = cells[currentCellIndex];

                for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
                {
                    MazeDirection direction = (MazeDirection)directionIndex;

                    if (currentCell.HasPassage(direction) == false)
                    {
                        continue;
                    }

                    Vector2Int neighborCoordinates = currentCell.Coordinates + MazeDirections.Delta(direction);
                    int neighborIndex = neighborCoordinates.y * side + neighborCoordinates.x;

                    if (distances[neighborIndex] == UnreachedDistance == false)
                    {
                        continue;
                    }

                    int neighborDistance = distances[currentCellIndex] + 1;
                    distances[neighborIndex] = neighborDistance;
                    pendingCells.Enqueue(neighborIndex);

                    if (neighborDistance > farthestDistance)
                    {
                        farthestDistance = neighborDistance;
                        farthestCellIndex = neighborIndex;
                    }
                }
            }

            return farthestCellIndex;
        }

        private int IndexOfNeighbor(int side, int cellIndex, MazeDirection direction)
        {
            Vector2Int delta = MazeDirections.Delta(direction);
            int neighborCoordinateX = cellIndex % side + delta.x;
            int neighborCoordinateY = cellIndex / side + delta.y;

            return neighborCoordinateY * side + neighborCoordinateX;
        }

        private void Shuffle<T>(List<T> values, System.Random random)
        {
            for (int index = values.Count - 1; index > 0; index--)
            {
                int swapIndex = random.Next(index + 1);
                T swapValue = values[index];
                values[index] = values[swapIndex];
                values[swapIndex] = swapValue;
            }
        }

        private struct SpanningTreeStep
        {
            public int CellIndex;
            public int ParentCellIndex;
            public MazeDirection DirectionToParent;

            public SpanningTreeStep(int cellIndex, int parentCellIndex, MazeDirection directionToParent)
            {
                CellIndex = cellIndex;
                ParentCellIndex = parentCellIndex;
                DirectionToParent = directionToParent;
            }
        }

        private struct LoopCandidate
        {
            public int CellIndex;
            public MazeDirection Direction;

            public LoopCandidate(int cellIndex, MazeDirection direction)
            {
                CellIndex = cellIndex;
                Direction = direction;
            }
        }
    }
}
