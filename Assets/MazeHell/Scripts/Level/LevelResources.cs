using UnityEngine;
using UnityEngine.AI;

namespace MazeHell.Level
{
    public class LevelResources : MonoBehaviour
    {
        private NavMeshData _navigationData;

        public void Setup(NavMeshData navigationData)
        {
            if (_navigationData != navigationData)
            {
                Release(_navigationData);
            }

            _navigationData = navigationData;
        }

        private void OnDestroy()
        {
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
