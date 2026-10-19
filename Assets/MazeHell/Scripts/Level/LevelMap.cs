using System;
using System.Collections.Generic;
using UnityEngine;

namespace MazeHell.Level
{
    public class LevelMap
    {
        private List<Bounds> _rooms = new List<Bounds>();
        private List<LevelMapPassage> _passages = new List<LevelMapPassage>();
        private HashSet<int> _discoveredRooms = new HashSet<int>();
        private int _currentRoomIndex = -1;
        private int _entranceRoomIndex = -1;
        private int _exitRoomIndex = -1;

        public IReadOnlyList<Bounds> Rooms => _rooms;
        public IReadOnlyList<LevelMapPassage> Passages => _passages;
        public int CurrentRoomIndex => _currentRoomIndex;
        public int EntranceRoomIndex => _entranceRoomIndex;
        public int ExitRoomIndex => _exitRoomIndex;

        public int AddRoom(Bounds bounds)
        {
            _rooms.Add(bounds);
            return _rooms.Count - 1;
        }

        public void Connect(int firstRoomIndex, int secondRoomIndex, params Vector3[] bends)
        {
            ValidateRoom(firstRoomIndex);
            ValidateRoom(secondRoomIndex);
            Vector2[] points = new Vector2[bends.Length + 2];
            Vector3 first = _rooms[firstRoomIndex].center;
            Vector3 second = _rooms[secondRoomIndex].center;
            points[0] = new Vector2(first.x, first.z);
            points[points.Length - 1] = new Vector2(second.x, second.z);

            for (int bendIndex = 0; bendIndex < bends.Length; bendIndex++)
            {
                points[bendIndex + 1] = new Vector2(bends[bendIndex].x, bends[bendIndex].z);
            }

            _passages.Add(new LevelMapPassage(firstRoomIndex, secondRoomIndex, points));
        }

        public void SetEndpoints(int entranceRoomIndex, int exitRoomIndex)
        {
            ValidateRoom(entranceRoomIndex);
            ValidateRoom(exitRoomIndex);
            _entranceRoomIndex = entranceRoomIndex;
            _exitRoomIndex = exitRoomIndex;
        }

        public bool IsRoomDiscovered(int roomIndex)
        {
            return _discoveredRooms.Contains(roomIndex);
        }

        public bool VisitPosition(Vector3 position)
        {
            for (int roomIndex = 0; roomIndex < _rooms.Count; roomIndex++)
            {
                Bounds bounds = _rooms[roomIndex];

                if (bounds.Contains(position) == false)
                {
                    continue;
                }

                if (_currentRoomIndex == roomIndex)
                {
                    return false;
                }

                foreach (LevelMapPassage passage in _passages)
                {
                    if (passage.Joins(_currentRoomIndex, roomIndex))
                    {
                        passage.Discover();
                    }
                }

                _currentRoomIndex = roomIndex;
                _discoveredRooms.Add(roomIndex);
                return true;
            }

            return false;
        }

        private void ValidateRoom(int roomIndex)
        {
            if (roomIndex < 0 || roomIndex >= _rooms.Count)
            {
                throw new ArgumentOutOfRangeException(nameof(roomIndex));
            }
        }
    }
}
