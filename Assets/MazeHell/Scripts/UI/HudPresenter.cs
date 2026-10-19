using System;
using MazeHell.Combat;
using MazeHell.Configuration;
using MazeHell.Player;
using MazeHell.Level;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.UI
{
    [DisallowMultipleComponent]
    public class HudPresenter : MonoBehaviour
    {
        private const string LevelSplashFormatKey = "level_splash_format";
        private const string KillObjectiveFormatKey = "level_objective_kill_format";

        private ObjectiveBanner _objective;
        private int _remainingEnemies;
        private bool _isExitDiscovered;

        private const float MinimalDashCooldown = 0.01f;

        private const float MinimalFlashDuration = 0.01f;

        private HudView _view;

        private PlayerRig _player;

        private InterfaceConfig _interface;

        private PlayerMovementConfig _movement;

        private Localization _localization;

        private float _damageFlashTimer;

        private float _splashRemaining;

        private bool _isSetupFinished;

        public void Setup(HudView view, PlayerRig player, InterfaceConfig ui, AnimationLibrary animations,
            HudMotionConfig hud, PlayerMovementConfig movement, ShotgunConfig gun, Localization localization)
        {
            if (view == null)
            {
                throw new ArgumentNullException(nameof(view));
            }

            if (player == null)
            {
                throw new ArgumentNullException(nameof(player));
            }

            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui));
            }

            if (animations == null)
            {
                throw new ArgumentNullException(nameof(animations));
            }

            if (hud == null)
            {
                throw new ArgumentNullException(nameof(hud));
            }

            if (movement == null)
            {
                throw new ArgumentNullException(nameof(movement));
            }

            if (gun == null)
            {
                throw new ArgumentNullException(nameof(gun));
            }

            if (localization == null)
            {
                throw new ArgumentNullException(nameof(localization));
            }

            if (view.Spring == null || view.Portrait == null || view.WeaponAnimator == null ||
                view.PortraitImage == null || view.WeaponImage == null || view.BarrelLeft == null ||
                view.BarrelRight == null || view.DashImage == null || view.HealthFill == null ||
                view.DamageOverlay == null || view.HealthText == null || view.Minimap == null ||
                view.SplashText == null || view.SplashPanel == null || view.ObjectiveText == null)
            {
                throw new InvalidOperationException($"{name}: hud view has unassigned bindings.");
            }

            UnsubscribeFromPlayer();

            _view = view;
            _objective = new ObjectiveBanner(view.ObjectiveText);
            _player = player;
            _interface = ui;
            _movement = movement;
            _localization = localization;
            _damageFlashTimer = 0f;
            _splashRemaining = 0f;
            _isSetupFinished = true;

            view.Spring.Setup(view.SpringRoot, view.WeaponRoot, view.CrosshairRoot, player.CameraMotion, hud);
            view.Spring.BindMotor(player.Motor);
            view.Portrait.Setup(view.PortraitImage, player.Health, animations, hud);
            view.WeaponAnimator.Setup(view.WeaponImage, player.Shotgun, animations, gun);

            player.Health.Changed += OnHealthChanged;
            player.Shotgun.Changed += OnShotgunChanged;
            player.Defense.Damaged += OnDefenseDamaged;

            OnHealthChanged(player.Health);
            OnShotgunChanged(player.Shotgun);

            view.DamageOverlay.color = BuildVignetteColor(0f);
            view.SplashPanel.SetActive(false);
        }

        public void LateTick(float deltaTime)
        {
            AssertSetup();

            float clampedDeltaTime = Mathf.Max(0f, deltaTime);

            _view.Spring.Tick(clampedDeltaTime);
            _view.Portrait.Tick(clampedDeltaTime);
            _view.WeaponAnimator.Tick(clampedDeltaTime);

            _view.Minimap.Tick(_player.transform.position);
            UpdateObjective();
            _objective.Tick(clampedDeltaTime);
            UpdateDashPip();
            UpdateVignette(clampedDeltaTime);
            UpdateSplash(clampedDeltaTime);
            _view.Screens.Tick(clampedDeltaTime);
        }

        public void ShowMap(LevelMap map)
        {
            AssertSetup();
            _view.Minimap.Setup(map);
        }

        public void ShowLevel(int level, Localization localization, LevelPresentationConfig presentation)
        {
            AssertSetup();

            if (localization == null)
            {
                throw new ArgumentNullException(nameof(localization));
            }

            if (presentation == null)
            {
                throw new ArgumentNullException(nameof(presentation));
            }

            _localization = localization;
            _view.SplashPanel.SetActive(true);
            _view.SplashText.text = string.Format(localization.Get(LevelSplashFormatKey), level);
            _splashRemaining = Mathf.Max(0f, presentation.LevelSplashDuration);

            _isExitDiscovered = false;
            _remainingEnemies = 0;
            _objective.Reset(localization.Get(presentation.ObjectiveKey));

            if (level == 1)
            {
                _view.Screens.ShowMonologue(localization.Get(presentation.FirstLevelMonologueKey));
            }
        }

        public void SetEnemyCount(int count)
        {
            AssertSetup();

            if (count < 0)
            {
                throw new ArgumentOutOfRangeException(nameof(count));
            }

            _remainingEnemies = count;

            if (_isExitDiscovered)
            {
                _objective.UpdateText(BuildKillObjective());
            }
        }

        private void UpdateObjective()
        {
            LevelMap map = _view.Minimap.Map;

            if (_isExitDiscovered || map == null || map.IsRoomDiscovered(map.ExitRoomIndex) == false)
            {
                return;
            }

            _isExitDiscovered = true;
            _objective.Change(BuildKillObjective());
        }

        private string BuildKillObjective()
        {
            return string.Format(_localization.Get(KillObjectiveFormatKey), _remainingEnemies);
        }

        private void OnHealthChanged(Health health)
        {
            float healthFraction = 0f;

            if (health.MaxHealth > 0f)
            {
                healthFraction = Mathf.Clamp01(health.CurrentHealth / health.MaxHealth);
            }

            _view.HealthFill.fillAmount = healthFraction;
            int currentHealth = Mathf.CeilToInt(health.CurrentHealth);
            _view.HealthText.text = string.Format(_localization.Get("hud_health_format"), currentHealth);
        }

        private void OnShotgunChanged(Shotgun gun)
        {
            UpdateBarrel(_view.BarrelLeft, gun.Ammo >= 1);
            UpdateBarrel(_view.BarrelRight, gun.Ammo >= 2);

        }

        private void OnDefenseDamaged(float damage)
        {
            _damageFlashTimer = _interface.DamageFlashDuration;
        }

        private void UpdateBarrel(Image barrel, bool isCharged)
        {
            if (isCharged)
            {
                barrel.sprite = _interface.BarrelFullSprite;

                return;
            }

            barrel.sprite = _interface.BarrelEmptySprite;
        }

        private void UpdateDashPip()
        {
            float dashCooldown = Mathf.Max(MinimalDashCooldown, _movement.DashCooldown);
            float cooldownFraction = Mathf.Clamp01(_player.Motor.DashCooldownRemaining / dashCooldown);

            _view.DashImage.fillAmount = 1f - cooldownFraction;
        }

        private void UpdateVignette(float deltaTime)
        {
            if (_damageFlashTimer > 0f)
            {
                _damageFlashTimer = Mathf.Max(0f, _damageFlashTimer - deltaTime);
            }

            float flashDuration = Mathf.Max(MinimalFlashDuration, _interface.DamageFlashDuration);
            float flashFraction = Mathf.Clamp01(_damageFlashTimer / flashDuration);

            _view.DamageOverlay.color = BuildVignetteColor(flashFraction);
        }

        private void UpdateSplash(float deltaTime)
        {
            if (_splashRemaining <= 0f)
            {
                return;
            }

            _splashRemaining -= deltaTime;

            if (_splashRemaining <= 0f)
            {
                _view.SplashPanel.SetActive(false);
            }
        }

        private Color BuildVignetteColor(float flashFraction)
        {
            Color color = _interface.DangerColor;
            color.a = _interface.MaxVignetteAlpha * flashFraction;

            return color;
        }

        private void UnsubscribeFromPlayer()
        {
            if (_player == null)
            {
                return;
            }

            _player.Health.Changed -= OnHealthChanged;
            _player.Shotgun.Changed -= OnShotgunChanged;
            _player.Defense.Damaged -= OnDefenseDamaged;
        }

        private void OnDisable()
        {
            if (_isSetupFinished == false)
            {
                return;
            }

            UnsubscribeFromPlayer();
        }

        private void AssertSetup()
        {
            if (_isSetupFinished == false)
            {
                throw new InvalidOperationException($"{name}: hud presenter is not initialized, call Setup first.");
            }
        }
    }
}
