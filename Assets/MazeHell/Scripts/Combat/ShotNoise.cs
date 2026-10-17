using System;
using UnityEngine;

namespace MazeHell.Combat
{
    public class ShotNoise
    {
        public event Action<Vector3, float> Emitted;

        public void Emit(Vector3 position, float radius)
        {
            if (radius < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(radius), $"Shot noise radius must be non-negative, got {radius}.");
            }

            Emitted?.Invoke(position, radius);
        }
    }
}
