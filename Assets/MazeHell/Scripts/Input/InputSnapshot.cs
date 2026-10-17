using UnityEngine;

namespace MazeHell.Input
{
    public struct InputSnapshot
    {
        private Vector2 _move;
        private Vector2 _look;
        private bool _isJumpPressed;
        private bool _isDashPressed;
        private bool _isFirePressed;
        private bool _isBlocking;
        private bool _isPausePressed;
        private bool _isReloadPressed;

        public InputSnapshot(Vector2 move, Vector2 look, bool isJumpPressed, bool isDashPressed,
            bool isFirePressed, bool isBlocking, bool isPausePressed, bool isReloadPressed = false)
        {
            _move = move;
            _look = look;
            _isJumpPressed = isJumpPressed;
            _isDashPressed = isDashPressed;
            _isFirePressed = isFirePressed;
            _isBlocking = isBlocking;
            _isPausePressed = isPausePressed;
            _isReloadPressed = isReloadPressed;
        }

        public Vector2 Move => _move;

        public Vector2 Look => _look;

        public bool JumpPressed => _isJumpPressed;

        public bool DashPressed => _isDashPressed;

        public bool FirePressed => _isFirePressed;

        public bool IsBlocking => _isBlocking;

        public bool PausePressed => _isPausePressed;

        public bool ReloadPressed => _isReloadPressed;
    }
}
