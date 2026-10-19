using UnityEngine;

namespace MazeHell.Level
{
    public class LevelMapPassage
    {
        private int _firstRoomIndex;
        private int _secondRoomIndex;
        private Vector2[] _points;
        private bool _isDiscovered;

        public Vector2[] Points => _points;
        public bool IsDiscovered => _isDiscovered;

        public LevelMapPassage(int firstRoomIndex, int secondRoomIndex, Vector2[] points)
        {
            _firstRoomIndex = firstRoomIndex;
            _secondRoomIndex = secondRoomIndex;
            _points = points;
        }

        public bool Joins(int firstRoomIndex, int secondRoomIndex)
        {
            return (_firstRoomIndex == firstRoomIndex && _secondRoomIndex == secondRoomIndex) ||
                (_firstRoomIndex == secondRoomIndex && _secondRoomIndex == firstRoomIndex);
        }

        public void Discover()
        {
            _isDiscovered = true;
        }
    }
}
