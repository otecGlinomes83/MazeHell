using System;
using MazeHell.Enemies;
using MazeHell.Input;
using MazeHell.Player;
using MazeHell.UI;

namespace MazeHell.Session
{
    public class GameLoop
    {
        private PlayerRig _player;
        private HudPresenter _presenter;

        public GameLoop(PlayerRig player, HudPresenter presenter)
        {
            if (player == null)
            {
                throw new ArgumentNullException(nameof(player), "Player rig is required.");
            }

            if (presenter == null)
            {
                throw new ArgumentNullException(nameof(presenter), "Hud presenter is required.");
            }

            _player = player;
            _presenter = presenter;
        }

        public void Tick(InputSnapshot input, float deltaTime, EnemyRig[] enemies)
        {
            _player.Look.Tick(input.Look);
            _player.Motor.Tick(input, deltaTime);
            _player.Defense.Tick(input.IsBlocking, deltaTime);
            _player.Shotgun.Tick(input, deltaTime);
            _player.MuzzleFlash.Tick(deltaTime);
            _player.Feedback.Tick();

            for (int enemyIndex = 0; enemyIndex < enemies.Length; enemyIndex++)
            {
                EnemyRig enemy = enemies[enemyIndex];
                enemy.Brain.Tick(deltaTime);
                enemy.Animator.Tick(deltaTime);
                enemy.Presentation.Tick();
            }
        }

        public void LateTick(float deltaTime)
        {
            _player.CameraMotion.Tick(_player.Motor.HorizontalSpeed, _player.Motor.IsGrounded, deltaTime);
            _presenter.LateTick(deltaTime);
        }
    }
}
