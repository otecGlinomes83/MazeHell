using System;
using UnityEngine;

namespace MazeHell.UI
{
    public class UiSpawner
    {
        private Transform _parent;
        private int _nextOrder = 20;

        public UiSpawner(Transform parent)
        {
            if (parent == null)
            {
                throw new ArgumentNullException(nameof(parent));
            }

            _parent = parent;
        }

        public T Spawn<T>(T prefab) where T : BaseWindow
        {
            if (prefab == null)
            {
                throw new ArgumentNullException(nameof(prefab));
            }

            T instance = UnityEngine.Object.Instantiate(prefab, _parent, false);

            if (instance.TryGetComponent(out Canvas canvas) == false)
            {
                throw new InvalidOperationException($"{prefab.name}: root Canvas is required.");
            }

            canvas.overrideSorting = true;
            canvas.sortingOrder = _nextOrder++;
            return instance;
        }

        public void Release(BaseWindow window)
        {
            if (window == null)
            {
                return;
            }

            window.gameObject.SetActive(false);
            UnityEngine.Object.Destroy(window.gameObject);
        }
    }
}
