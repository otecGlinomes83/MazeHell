using System;
using UnityEngine;

namespace MazeHell.Level
{
    public class MazeCell
    {
        private int _index;
        private Vector2Int _coordinates;
        private bool _northOpen;
        private bool _eastOpen;
        private bool _southOpen;
        private bool _westOpen;
        private int _passageCount;
        private bool _isDeadEnd;

        public MazeCell(int index, Vector2Int coordinates, bool northOpen, bool eastOpen, bool southOpen, bool westOpen)
        {
            _index = index;
            _coordinates = coordinates;
            _northOpen = northOpen;
            _eastOpen = eastOpen;
            _southOpen = southOpen;
            _westOpen = westOpen;
            _passageCount = Convert.ToInt32(northOpen) + Convert.ToInt32(eastOpen) + Convert.ToInt32(southOpen) + Convert.ToInt32(westOpen);
            _isDeadEnd = _passageCount == 1;
        }

        public int Index => _index;

        public Vector2Int Coordinates => _coordinates;

        public int PassageCount => _passageCount;

        public bool IsDeadEnd => _isDeadEnd;

        public bool HasPassage(MazeDirection direction)
        {
            switch (direction)
            {
                case MazeDirection.North:
                    return _northOpen;
                case MazeDirection.East:
                    return _eastOpen;
                case MazeDirection.South:
                    return _southOpen;
                case MazeDirection.West:
                    return _westOpen;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Unknown maze direction {direction}.");
            }
        }
    }
}
