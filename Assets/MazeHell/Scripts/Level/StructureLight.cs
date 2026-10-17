using UnityEngine;

namespace MazeHell
{
    [DisallowMultipleComponent]
    [RequireComponent(typeof(Light))]
    public sealed class StructureLight : MonoBehaviour
    {
        public enum Role { Room, Corridor }
        public Role role;
    }
}
