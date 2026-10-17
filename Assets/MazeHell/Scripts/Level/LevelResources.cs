using UnityEngine;
using UnityEngine.AI;

namespace MazeHell.Level
{
    public class LevelResources : MonoBehaviour
    {
        private Mesh[] _meshes;
        private NavMeshData _navigationData;

        public void Setup(Mesh[] meshes, NavMeshData navigationData)
        {
            _meshes = meshes;
            _navigationData = navigationData;
        }

        private void OnDestroy()
        {
            if (_meshes != null)
            {
                for (int i = 0; i < _meshes.Length; i++)
                {
                    Release(_meshes[i]);
                }
            }

            Release(_navigationData);
        }

        private void Release(UnityEngine.Object resource)
        {
            if (resource == null)
            {
                return;
            }

            if (Application.isPlaying)
            {
                Destroy(resource);
                return;
            }

            DestroyImmediate(resource);
        }
    }
}
