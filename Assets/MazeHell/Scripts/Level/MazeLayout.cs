using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeHell.Level
{
    public class MazeLayout
    {
        public const int MinimumSide = 4;

        private const int UnreachedDistance = -1;

        private int _side;
        private int _seed;
        private int _startCellIndex;
        private int _exitCellIndex;
        private int _size;
        private MazeCell[] _cells;

        public MazeLayout(int side, int seed, int startCellIndex, int exitCellIndex, MazeCell[] cells)
        {
            if (side < MinimumSide)
            {
                throw new ArgumentOutOfRangeException(nameof(side), side, $"Side must be at least {MinimumSide}.");
            }

            if (cells == null)
            {
                throw new ArgumentNullException(nameof(cells));
            }

            if (cells.Length == side * side == false)
            {
                throw new ArgumentException($"Cells length {cells.Length} must match side squared {side * side}.", nameof(cells));
            }

            _cells = cells;

            ValidateCellIndex(startCellIndex, nameof(startCellIndex));
            ValidateCellIndex(exitCellIndex, nameof(exitCellIndex));

            if (startCellIndex == exitCellIndex)
            {
                throw new ArgumentException($"Start and exit must differ, both are {startCellIndex}.");
            }

            _side = side;
            _seed = seed;
            _startCellIndex = startCellIndex;
            _exitCellIndex = exitCellIndex;
            _size = cells.Length;
        }

        public int Side => _side;

        public int Size => _size;

        public int Seed => _seed;

        public int StartCellIndex => _startCellIndex;

        public int ExitCellIndex => _exitCellIndex;

        public MazeCell[] Cells => _cells;

        public MazeCell GetStartCell()
        {
            return _cells[_startCellIndex];
        }

        public MazeCell GetExitCell()
        {
            return _cells[_exitCellIndex];
        }

        public MazeCell GetCell(int cellIndex)
        {
            ValidateCellIndex(cellIndex, nameof(cellIndex));

            return _cells[cellIndex];
        }

        public bool TryGetCell(Vector2Int coordinates, out MazeCell cell)
        {
            if (coordinates.x < 0 || coordinates.x >= _side || coordinates.y < 0 || coordinates.y >= _side)
            {
                cell = null;
                return false;
            }

            cell = _cells[IndexOfCoordinates(coordinates)];
            return true;
        }

        public bool TryGetNeighbor(MazeCell cell, MazeDirection direction, out MazeCell neighbor)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            if (cell.HasPassage(direction) == false)
            {
                neighbor = null;
                return false;
            }

            return TryGetCell(cell.Coordinates + MazeDirections.Delta(direction), out neighbor);
        }

        public bool TryGetNeighborIndex(int cellIndex, MazeDirection direction, out int neighborIndex)
        {
            ValidateCellIndex(cellIndex, nameof(cellIndex));

            MazeCell neighbor;
            bool neighborFound = TryGetNeighbor(_cells[cellIndex], direction, out neighbor);

            if (neighborFound == false)
            {
                neighborIndex = UnreachedDistance;
                return false;
            }

            neighborIndex = neighbor.Index;
            return true;
        }

        public IList<MazeCell> GetNeighbors(MazeCell cell)
        {
            if (cell == null)
            {
                throw new ArgumentNullException(nameof(cell));
            }

            List<MazeCell> neighbors = new List<MazeCell>(MazeDirections.Count);

            for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
            {
                MazeCell neighbor;
                if (TryGetNeighbor(cell, (MazeDirection)directionIndex, out neighbor))
                {
                    neighbors.Add(neighbor);
                }
            }

            return neighbors;
        }

        public int GetDistance(int fromCellIndex, int toCellIndex)
        {
            ValidateCellIndex(fromCellIndex, nameof(fromCellIndex));
            ValidateCellIndex(toCellIndex, nameof(toCellIndex));

            int[] distances = ComputeDistancesFrom(fromCellIndex);

            return distances[toCellIndex];
        }

        public bool IsConnected()
        {
            int[] distances = ComputeDistancesFrom(_startCellIndex);

            for (int cellIndex = 0; cellIndex < _cells.Length; cellIndex++)
            {
                if (distances[cellIndex] == UnreachedDistance)
                {
                    return false;
                }
            }

            return true;
        }

        public int CountDeadEnds()
        {
            int deadEndCount = 0;

            for (int cellIndex = 0; cellIndex < _cells.Length; cellIndex++)
            {
                if (_cells[cellIndex].IsDeadEnd)
                {
                    deadEndCount += 1;
                }
            }

            return deadEndCount;
        }

        public int CountPassages()
        {
            int passageCount = 0;

            for (int cellIndex = 0; cellIndex < _cells.Length; cellIndex++)
            {
                passageCount += _cells[cellIndex].PassageCount;
            }

            return passageCount;
        }

        private int[] ComputeDistancesFrom(int fromCellIndex)
        {
            int[] distances = new int[_cells.Length];

            for (int cellIndex = 0; cellIndex < distances.Length; cellIndex++)
            {
                distances[cellIndex] = UnreachedDistance;
            }

            Queue<int> pendingCells = new Queue<int>();
            distances[fromCellIndex] = 0;
            pendingCells.Enqueue(fromCellIndex);

            while (pendingCells.Count > 0)
            {
                int currentCellIndex = pendingCells.Dequeue();
                MazeCell currentCell = _cells[currentCellIndex];

                for (int directionIndex = 0; directionIndex < MazeDirections.Count; directionIndex++)
                {
                    MazeDirection direction = (MazeDirection)directionIndex;

                    if (currentCell.HasPassage(direction) == false)
                    {
                        continue;
                    }

                    int neighborIndex = IndexOfCoordinates(currentCell.Coordinates + MazeDirections.Delta(direction));

                    if (distances[neighborIndex] == UnreachedDistance == false)
                    {
                        continue;
                    }

                    distances[neighborIndex] = distances[currentCellIndex] + 1;
                    pendingCells.Enqueue(neighborIndex);
                }
            }

            return distances;
        }

        private int IndexOfCoordinates(Vector2Int coordinates)
        {
            return coordinates.y * _side + coordinates.x;
        }

        private void ValidateCellIndex(int cellIndex, string argumentName)
        {
            if (cellIndex < 0 || cellIndex >= _cells.Length)
            {
                throw new ArgumentOutOfRangeException(argumentName, cellIndex, $"Cell index must be within [0, {_cells.Length}).");
            }
        }
    }
}
