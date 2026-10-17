using Unity.AI.Navigation;
using UnityEngine;

namespace MazeHell.Level
{
    [DisallowMultipleComponent]
    public class LevelRoot : MonoBehaviour
    {
        [SerializeField] private Transform _geometryRoot;
        [SerializeField] private NavMeshSurface _navigation;
        [SerializeField] private LevelResources _resources;

        public Transform GeometryRoot => _geometryRoot;
        public NavMeshSurface Navigation => _navigation;
        public LevelResources Resources => _resources;
    }
}
