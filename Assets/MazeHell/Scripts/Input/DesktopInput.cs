using UnityEngine;
using UnityEngine.InputSystem;

namespace MazeHell.Input
{
    [DisallowMultipleComponent]
    public class DesktopInput : MonoBehaviour
    {
        public InputSnapshot Poll()
        {
            Keyboard keyboard = Keyboard.current;
            Mouse mouse = Mouse.current;

            Vector2 move = ReadMove(keyboard);
            Vector2 look = ReadLook(mouse);
            bool isJumpPressed = ReadJumpPressed(keyboard);
            bool isDashPressed = ReadDashPressed(keyboard);
            bool isFirePressed = ReadFirePressed(mouse);
            bool isBlocking = ReadBlocking(mouse);
            bool isPausePressed = ReadPausePressed(keyboard);

            return new InputSnapshot(move, look, isJumpPressed, isDashPressed, isFirePressed, isBlocking, isPausePressed);
        }

        private Vector2 ReadMove(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return Vector2.zero;
            }

            float moveX = 0f;
            float moveY = 0f;

            if (keyboard.dKey.isPressed)
            {
                moveX += 1f;
            }

            if (keyboard.aKey.isPressed)
            {
                moveX -= 1f;
            }

            if (keyboard.wKey.isPressed)
            {
                moveY += 1f;
            }

            if (keyboard.sKey.isPressed)
            {
                moveY -= 1f;
            }

            return new Vector2(moveX, moveY);
        }

        private Vector2 ReadLook(Mouse mouse)
        {
            if (mouse == null)
            {
                return Vector2.zero;
            }

            return mouse.delta.ReadValue();
        }

        private bool ReadJumpPressed(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return false;
            }

            return keyboard.spaceKey.wasPressedThisFrame;
        }

        private bool ReadDashPressed(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return false;
            }

            return keyboard.leftShiftKey.wasPressedThisFrame;
        }

        private bool ReadFirePressed(Mouse mouse)
        {
            if (mouse == null)
            {
                return false;
            }

            return mouse.leftButton.wasPressedThisFrame;
        }

        private bool ReadBlocking(Mouse mouse)
        {
            if (mouse == null)
            {
                return false;
            }

            return mouse.rightButton.isPressed;
        }

        private bool ReadPausePressed(Keyboard keyboard)
        {
            if (keyboard == null)
            {
                return false;
            }

            return keyboard.escapeKey.wasPressedThisFrame;
        }
    }
}
