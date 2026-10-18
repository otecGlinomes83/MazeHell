using System;
using MazeHell.Configuration;
using MazeHell.Player;
using MazeHell.Session;
using UnityEngine;
using UnityEngine.UI;

namespace MazeHell.UI
{
    public class HudFactory
    {
        private HudView _hudPrefab;
        private InterfaceConfig _interface;
        private AnimationLibrary _animations;
        private HudMotionConfig _hud;
        private PlayerMovementConfig _movement;
        private ShotgunConfig _gun;
        private CameraMotionConfig _camera;

        public HudFactory(HudView hudPrefab, InterfaceConfig ui, AnimationLibrary animations, HudMotionConfig hud,
            PlayerMovementConfig movement, ShotgunConfig gun, CameraMotionConfig camera)
        {
            if (hudPrefab == null)
            {
                throw new ArgumentNullException(nameof(hudPrefab), "Hud prefab is required.");
            }

            if (ui == null)
            {
                throw new ArgumentNullException(nameof(ui), "Interface config is required.");
            }

            if (animations == null)
            {
                throw new ArgumentNullException(nameof(animations), "Animation library is required.");
            }

            if (hud == null)
            {
                throw new ArgumentNullException(nameof(hud), "Hud motion config is required.");
            }

            if (movement == null)
            {
                throw new ArgumentNullException(nameof(movement), "Player movement config is required.");
            }

            if (gun == null)
            {
                throw new ArgumentNullException(nameof(gun), "Shotgun config is required.");
            }

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera), "Camera motion config is required.");
            }

            _hudPrefab = hudPrefab;
            _interface = ui;
            _animations = animations;
            _hud = hud;
            _movement = movement;
            _gun = gun;
            _camera = camera;
        }

        public HudView Create(PlayerRig player, Localization localization, RecordStorage records, Transform parent)
        {
            HudView view = UnityEngine.Object.Instantiate(_hudPrefab, parent);
            view.name = "Hud";
            if (view.TryGetComponent(out CanvasScaler scaler) == false)
            {
                throw new InvalidOperationException("HUD prefab must contain CanvasScaler.");
            }

            scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = _interface.ReferenceResolution;
            scaler.matchWidthOrHeight = _interface.MatchWidthOrHeight;
            view.CrosshairRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, _interface.CrosshairSizePixels);
            view.CrosshairRoot.SetSizeWithCurrentAnchors(RectTransform.Axis.Vertical, _interface.CrosshairSizePixels);
            SafeArea safeArea = view.SafeRoot.GetComponent<SafeArea>();
            safeArea.Setup(view.SafeRoot);

            HudPresenter presenter = view.gameObject.AddComponent<HudPresenter>();

            presenter.Setup(view, player, _interface, _animations, _hud, _movement, _gun, localization);

            HudScreens screens = view.gameObject.AddComponent<HudScreens>();

            screens.Setup(view, localization, records, _interface, _camera);
            view.SetupPresentation(presenter, screens);
            return view;
        }
    }
}
