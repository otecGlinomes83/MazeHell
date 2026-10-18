using System;
using MazeHell.Configuration;
using MazeHell.Input;
using UnityEngine;

namespace MazeHell.Combat
{
    [DisallowMultipleComponent]
    public class Shotgun : MonoBehaviour
    {
        public event Action<Shotgun> Changed;

        public event Action<bool> Fired;

        public event Action<Shotgun> ReloadStarted;

        private const float ConeHalfScale = 0.5f;

        private ShotgunConfig _config;
        private Camera _aimCamera;
        private ShotNoise _noise;
        private bool _isSetupFinished;

        private DamageTarget[] _shotTargets;
        private int _shotTargetCount;
        private int _ammo;
        private ShotgunState _state;
        private bool _isBlocking;
        private float _shotIntervalTimer;
        private float _volleyWindowTimer;
        private float _reloadTimer;

        public int Ammo => _ammo;

        public ShotgunState State => _state;

        public bool IsBlocking => _isBlocking;

        public float ReloadRemaining => _reloadTimer;

        public void Setup(ShotgunConfig config, Camera aimCamera, ShotNoise noise)
        {
            if (config == null)
            {
                throw new ArgumentNullException(nameof(config), $"{name}: shotgun config is required.");
            }

            if (aimCamera == null)
            {
                throw new ArgumentNullException(nameof(aimCamera), $"{name}: aim camera is required.");
            }

            if (noise == null)
            {
                throw new ArgumentNullException(nameof(noise), $"{name}: shot noise is required.");
            }

            _config = config;
            _aimCamera = aimCamera;
            _noise = noise;
            _isSetupFinished = true;

            _shotTargets = new DamageTarget[config.PelletCount];
            _ammo = config.BarrelCount;
            _state = ShotgunState.Ready;
            _isBlocking = false;
            _shotIntervalTimer = 0f;
            _volleyWindowTimer = 0f;
            _reloadTimer = 0f;
        }

        public void Tick(InputSnapshot input, float deltaTime)
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

            _shotIntervalTimer = Mathf.Max(0f, _shotIntervalTimer - deltaTime);
            _volleyWindowTimer = Mathf.Max(0f, _volleyWindowTimer - deltaTime);

            UpdateBlockState(input.IsBlocking);
            UpdateReload(deltaTime);

            if (input.ReloadPressed && _ammo < _config.BarrelCount && _reloadTimer <= 0f && _isBlocking == false)
            {
                StartReload();
            }

            if (input.FirePressed && _ammo > 0 && _reloadTimer > 0f && _isBlocking == false)
            {
                _reloadTimer = 0f;
                Transition(ShotgunState.Ready);
            }

            UpdateFire(input);
        }

        private void UpdateBlockState(bool isBlocking)
        {
            _isBlocking = isBlocking;

            if (isBlocking && _state != ShotgunState.Blocking)
            {
                Transition(ShotgunState.Blocking);

                return;
            }

            if (isBlocking == false && _state == ShotgunState.Blocking)
            {
                if (_reloadTimer > 0f)
                {
                    Transition(ShotgunState.Reloading);
                }
                else
                {
                    Transition(ShotgunState.Ready);
                }
            }
        }

        private void UpdateReload(float deltaTime)
        {
            if (_reloadTimer <= 0f)
            {
                return;
            }

            _reloadTimer = Mathf.Max(0f, _reloadTimer - deltaTime);

            if (_reloadTimer > 0f)
            {
                return;
            }

            _ammo = _config.BarrelCount;

            if (_isBlocking == false)
            {
                Transition(ShotgunState.Ready);

                return;
            }

            Changed?.Invoke(this);
        }

        private void UpdateFire(InputSnapshot input)
        {
            if (input.FirePressed == false || _state != ShotgunState.Ready || _shotIntervalTimer > 0f || _ammo <= 0)
            {
                return;
            }

            bool isVolley = _volleyWindowTimer > 0f;

            _ammo -= 1;
            _shotIntervalTimer = _config.MinShotInterval;
            _volleyWindowTimer = _config.VolleyWindow;

            FirePellets(isVolley);

            _noise.Emit(_aimCamera.transform.position, _config.NoiseRadius);

            Fired?.Invoke(isVolley);
            Changed?.Invoke(this);

            if (_ammo <= 0)
            {
                StartReload();
            }
        }

        private void StartReload()
        {
            _reloadTimer = _config.ReloadDuration;

            Transition(ShotgunState.Reloading);

            ReloadStarted?.Invoke(this);
        }

        private void Transition(ShotgunState state)
        {
            if (_state == state)
            {
                return;
            }

            _state = state;

            Changed?.Invoke(this);
        }

        private void FirePellets(bool isVolley)
        {
            _shotTargetCount = 0;
            Transform aimTransform = _aimCamera.transform;
            float knockbackSpeed = _config.EnemyKnockbackSpeed;

            if (isVolley)
            {
                knockbackSpeed *= _config.VolleyKnockbackMultiplier;
            }

            for (int pelletIndex = 0; pelletIndex < _config.PelletCount; pelletIndex += 1)
            {
                Vector2 spreadOffset = UnityEngine.Random.insideUnitCircle * (_config.SpreadAngle * ConeHalfScale);
                Quaternion spreadRotation = Quaternion.Euler(spreadOffset.x, spreadOffset.y, 0f);
                Vector3 direction = aimTransform.rotation * spreadRotation * Vector3.forward;

                if (Physics.Raycast(aimTransform.position, direction, out RaycastHit hit, _config.MaxRange, _config.HitMask, QueryTriggerInteraction.Ignore) == false)
                {
                    continue;
                }

                if (hit.collider.TryGetComponent(out DamageTarget target) == false)
                {
                    continue;
                }

                float targetKnockback = knockbackSpeed;

                for (int targetIndex = 0; targetIndex < _shotTargetCount; targetIndex++)
                {
                    if (_shotTargets[targetIndex] == target)
                    {
                        targetKnockback = 0f;
                        break;
                    }
                }

                if (targetKnockback > 0f)
                {
                    _shotTargets[_shotTargetCount] = target;
                    _shotTargetCount += 1;
                }

                target.Receive(_config.PelletDamage, direction, targetKnockback);
            }

            Array.Clear(_shotTargets, 0, _shotTargetCount);
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: shotgun is not initialized, call Setup first.");
            }
        }
    }
}
