using System;
using MazeHell.Configuration;
using UnityEngine;

namespace MazeHell.Enemies
{
    [DisallowMultipleComponent]
    public class EnemySpriteAnimator : MonoBehaviour
    {
        private enum DisplayMode
        {
            Idle,
            Walk,
            Windup,
            Lunge,
            Corpse
        }

        private const float DirectionDotThreshold = 0.7071f;
        private const float MinDirectionSqrMagnitude = 0.0001f;

        [Min(0.1f)]
        [SerializeField] private float _worldHeight = 2.8f;

        [Min(0.1f)]
        [SerializeField] private float _corpseWorldHeight = 0.8f;

        private SpriteRenderer _spriteRenderer;
        private Transform _root;
        private Camera _viewCamera;
        private EnemySpriteSetData _enemySet;
        private EnemyConfig _config;
        private bool _isSetupFinished;

        private DisplayMode _mode;
        private float _modeTimer;
        private float _painTimer;
        private Sprite _currentSprite;

        public void Setup(SpriteRenderer spriteRenderer, Transform root, Camera viewCamera, AnimationLibrary library, EnemyConfig config)
        {
            if (spriteRenderer == null)
            {
                throw new ArgumentNullException(nameof(spriteRenderer), $"{name}: sprite renderer is required.");
            }

            if (root == null)
            {
                throw new ArgumentNullException(nameof(root), $"{name}: root transform is required.");
            }

            if (viewCamera == null)
            {
                throw new ArgumentNullException(nameof(viewCamera), $"{name}: view camera is required.");
            }

            if (library == null)
            {
                throw new ArgumentNullException(nameof(library), $"{name}: animation library is required.");
            }

            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: enemy config is required.");
            }

            _spriteRenderer = spriteRenderer;
            _root = root;
            _viewCamera = viewCamera;
            _enemySet = library.EnemySet;
            _config = config;
            _isSetupFinished = true;

            _mode = DisplayMode.Idle;
            _modeTimer = 0f;
            _painTimer = 0f;
            _currentSprite = null;

            if (_enemySet.WalkFrontSprites.Length > 0 && _enemySet.WalkFrontSprites[0] != null)
            {
                float scale = _worldHeight / _enemySet.WalkFrontSprites[0].bounds.size.y;
                _spriteRenderer.transform.localScale = Vector3.one * scale;
                _spriteRenderer.transform.localPosition = Vector3.up * (-_enemySet.WalkFrontSprites[0].bounds.min.y * scale);
            }
        }

        public void Tick(float deltaTime)
        {
            AssertSetup();

            if (float.IsFinite(deltaTime) == false)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), $"{name}: delta time must be finite, got {deltaTime}.");
            }

            if (deltaTime < 0f)
            {
                throw new ArgumentOutOfRangeException(nameof(deltaTime), $"{name}: delta time must be non-negative, got {deltaTime}.");
            }

            BillboardTowardsCamera();

            if (_mode == DisplayMode.Corpse)
            {
                ShowSprite(_enemySet.CorpseSprite);

                return;
            }

            if (_painTimer > 0f)
            {
                _painTimer = Mathf.Max(0f, _painTimer - deltaTime);

                if (ShowPainFrames())
                {
                    return;
                }
            }

            _modeTimer += deltaTime;
            ShowBaseMode();
        }

        public void PlayIdle()
        {
            AssertSetup();
            SetMode(DisplayMode.Idle);
        }

        public void PlayWalk()
        {
            AssertSetup();
            SetMode(DisplayMode.Walk);
        }

        public void PlayWindup()
        {
            AssertSetup();
            SetMode(DisplayMode.Windup);
        }

        public void PlayLunge()
        {
            AssertSetup();
            SetMode(DisplayMode.Lunge);
        }

        public void PlayPain()
        {
            AssertSetup();

            _painTimer = _config.FlinchTime;
        }

        public void PlayCorpse()
        {
            AssertSetup();

            if (_mode == DisplayMode.Corpse)
            {
                return;
            }

            _mode = DisplayMode.Corpse;
            _modeTimer = 0f;
            _painTimer = 0f;
            if (_enemySet.CorpseSprite != null)
            {
                float scale = _corpseWorldHeight / _enemySet.CorpseSprite.bounds.size.y;
                _spriteRenderer.transform.localScale = Vector3.one * scale;
                _spriteRenderer.transform.localPosition = Vector3.up *
                    (_enemySet.CorpseGroundOffset - _enemySet.CorpseSprite.bounds.min.y * scale);
            }

            BillboardTowardsCamera();
            ShowSprite(_enemySet.CorpseSprite);
            _spriteRenderer.flipX = false;
        }

        private void SetMode(DisplayMode mode)
        {
            if (_mode == mode)
            {
                return;
            }

            _mode = mode;
            _modeTimer = 0f;
        }

        private void BillboardTowardsCamera()
        {
            Vector3 cameraEulerAngles = _viewCamera.transform.eulerAngles;

            _spriteRenderer.transform.rotation = Quaternion.Euler(0f, cameraEulerAngles.y, 0f);
        }

        private bool ShowPainFrames()
        {
            Sprite[] painFrames = _enemySet.PainSprites;

            if (painFrames == null || painFrames.Length == 0)
            {
                return false;
            }

            float painElapsed = _config.FlinchTime - _painTimer;
            int frameIndex = Mathf.Clamp(Mathf.FloorToInt(painElapsed / _enemySet.AttackFrameInterval), 0, painFrames.Length - 1);

            ShowSprite(painFrames[frameIndex]);

            return true;
        }

        private void ShowBaseMode()
        {
            switch (_mode)
            {
                case DisplayMode.Idle:
                    ShowDirectionalFrame(false);
                    break;
                case DisplayMode.Walk:
                    ShowDirectionalFrame(true);
                    break;
                case DisplayMode.Windup:
                    ShowAttackFrames(_enemySet.WindupSprites);
                    break;
                case DisplayMode.Lunge:
                    ShowAttackFrames(_enemySet.LungeSprites);
                    break;
                case DisplayMode.Corpse:
                    ShowSprite(_enemySet.CorpseSprite);
                    break;
            }
        }

        private void ShowDirectionalFrame(bool isWalking)
        {
            Vector3 toCamera = _viewCamera.transform.position - _root.position;
            toCamera.y = 0f;

            Vector3 forward = _root.forward;
            forward.y = 0f;

            float forwardDot = 0f;
            float sideDot = 0f;

            if (toCamera.sqrMagnitude > MinDirectionSqrMagnitude && forward.sqrMagnitude > MinDirectionSqrMagnitude)
            {
                toCamera.Normalize();
                forward.Normalize();
                forwardDot = Vector3.Dot(forward, toCamera);
                sideDot = Vector3.Dot(new Vector3(forward.z, 0f, -forward.x), toCamera);
            }

            Sprite[] frames;
            _spriteRenderer.flipX = false;

            if (forwardDot > DirectionDotThreshold)
            {
                frames = _enemySet.WalkFrontSprites;
            }
            else if (forwardDot < -DirectionDotThreshold)
            {
                frames = _enemySet.WalkBackSprites;
            }
            else
            {
                frames = _enemySet.WalkSideSprites;
                _spriteRenderer.flipX = sideDot < 0f;
            }

            if (frames == null || frames.Length == 0)
            {
                return;
            }

            if (isWalking)
            {
                int frameIndex = Mathf.FloorToInt(_modeTimer / _enemySet.WalkFrameInterval) % frames.Length;

                ShowSprite(frames[frameIndex]);

                return;
            }

            ShowSprite(frames[0]);
        }

        private void ShowAttackFrames(Sprite[] frames)
        {
            if (frames == null || frames.Length == 0)
            {
                return;
            }

            int frameIndex = Mathf.Clamp(Mathf.FloorToInt(_modeTimer / _enemySet.AttackFrameInterval), 0, frames.Length - 1);

            ShowSprite(frames[frameIndex]);
        }

        private void ShowSprite(Sprite sprite)
        {
            if (_currentSprite == sprite)
            {
                return;
            }

            _currentSprite = sprite;
            _spriteRenderer.sprite = sprite;
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: enemy sprite animator is not initialized, call Setup first.");
            }
        }
    }
}
