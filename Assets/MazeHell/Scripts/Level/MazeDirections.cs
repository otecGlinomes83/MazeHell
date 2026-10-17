using System;
using UnityEngine;

namespace MazeHell.Level
{
    public static class MazeDirections
    {
        public const int Count = 4;

        public static MazeDirection[] GetAll()
        {
            return new MazeDirection[]
            {
                MazeDirection.North,
                MazeDirection.East,
                MazeDirection.South,
                MazeDirection.West,
            };
        }

        public static MazeDirection Opposite(MazeDirection direction)
        {
            switch (direction)
            {
                case MazeDirection.North:
                    return MazeDirection.South;
                case MazeDirection.East:
                    return MazeDirection.West;
                case MazeDirection.South:
                    return MazeDirection.North;
                case MazeDirection.West:
                    return MazeDirection.East;
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Unknown maze direction {direction}.");
            }
        }

        public static Vector2Int Delta(MazeDirection direction)
        {
            switch (direction)
            {
                case MazeDirection.North:
                    return new Vector2Int(0, 1);
                case MazeDirection.East:
                    return new Vector2Int(1, 0);
                case MazeDirection.South:
                    return new Vector2Int(0, -1);
                case MazeDirection.West:
                    return new Vector2Int(-1, 0);
                default:
                    throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Unknown maze direction {direction}.");
            }
        }
    }
}
