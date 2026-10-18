using System;
using UnityEngine;

namespace MazeHell.Level
{
    [DisallowMultipleComponent]
    public class DecorationBillboard : MonoBehaviour
    {
        private Camera _camera;

        public void Setup(Camera camera)
        {
            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera));
            }

            _camera = camera;
            FaceCamera();
        }

        private void LateUpdate()
        {
            FaceCamera();
        }

        private void FaceCamera()
        {
            if (_camera == null)
            {
                return;
            }

            Vector3 direction = _camera.transform.position - transform.position;
            direction.y = 0f;

            if (direction.sqrMagnitude > 0.0001f)
            {
                transform.rotation = Quaternion.LookRotation(direction);
            }
        }
    }
}
