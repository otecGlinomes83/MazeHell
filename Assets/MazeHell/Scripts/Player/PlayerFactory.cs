using System;
using MazeHell.Audio;
using MazeHell.Combat;
using MazeHell.Configuration;
using MazeHell.Presentation;
using UnityEngine;

namespace MazeHell.Player
{
    public class PlayerFactory
    {
        private PlayerRig _playerPrefab;
        private PlayerMovementConfig _movement;
        private PlayerCombatConfig _combat;
        private ShotgunConfig _shotgun;
        private CameraMotionConfig _camera;
        private GameAudio _audio;
        private ShotNoise _noise;

        public PlayerFactory(PlayerRig playerPrefab, PlayerMovementConfig movement, PlayerCombatConfig combat,
            ShotgunConfig shotgun, CameraMotionConfig camera, GameAudio audio, ShotNoise noise)
        {
            if (playerPrefab == null)
            {
                throw new ArgumentNullException(nameof(playerPrefab), "Player prefab is required.");
            }

            if (movement == null)
            {
                throw new ArgumentNullException(nameof(movement), "Player movement config is required.");
            }

            if (combat == null)
            {
                throw new ArgumentNullException(nameof(combat), "Player combat config is required.");
            }

            if (shotgun == null)
            {
                throw new ArgumentNullException(nameof(shotgun), "Shotgun config is required.");
            }

            if (camera == null)
            {
                throw new ArgumentNullException(nameof(camera), "Camera motion config is required.");
            }

            if (audio == null)
            {
                throw new ArgumentNullException(nameof(audio), "Game audio is required.");
            }

            if (noise == null)
            {
                throw new ArgumentNullException(nameof(noise), "Shot noise is required.");
            }

            _playerPrefab = playerPrefab;
            _movement = movement;
            _combat = combat;
            _shotgun = shotgun;
            _camera = camera;
            _audio = audio;
            _noise = noise;
        }

        public PlayerRig Create(Vector3 position, Quaternion rotation, Transform parent)
        {
            PlayerRig player = UnityEngine.Object.Instantiate(_playerPrefab, position, rotation, parent);
            player.name = "Player";

            player.Health.Setup(_combat.MaxHealth);
            player.Motor.Setup(player.Controller, _movement);
            player.LookPivot.localPosition = new Vector3(player.LookPivot.localPosition.x, _movement.EyeHeight, player.LookPivot.localPosition.z);
            player.Look.Setup(player.transform, player.LookPivot, _camera.MouseSensitivity, _camera.PitchLimitDegrees);
            player.Shotgun.Setup(_shotgun, player.Camera, _noise);
            player.Defense.Setup(_combat, player.Health, player.Motor);
            player.CameraMotion.Setup(player.MotionPivot, player.Camera, _camera);

            MuzzleFlash muzzleFlash = player.gameObject.AddComponent<MuzzleFlash>();
            muzzleFlash.Setup(player.MuzzleLight, player.Shotgun, _shotgun);

            PlayerFeedback feedback = player.gameObject.AddComponent<PlayerFeedback>();
            feedback.Setup(player, _audio);
            player.SetupFeedback(muzzleFlash, feedback);

            return player;
        }

        public void RebindLook(PlayerRig player, float sensitivity)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), "Player rig is required.");
            }

            player.Look.Setup(player.transform, player.LookPivot, sensitivity, _camera.PitchLimitDegrees);
        }
    }
}
