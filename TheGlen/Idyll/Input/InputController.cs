using System;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Input;

namespace Idyll.Input
{
    public class KeyEventArgs : EventArgs
    {
        public readonly Keys Key;
        public KeyEventArgs(Keys key)
        {
            this.Key = key;
        }
    }

    public class MouseMoveEventArgs : EventArgs
    {
        public readonly Vector2 Distance;
        public MouseMoveEventArgs(Vector2 distance)
        {
            Distance = distance;
        }
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class InputController : Microsoft.Xna.Framework.GameComponent
    {
        public event EventHandler<KeyEventArgs> OnKeyDown;
        public event EventHandler<KeyEventArgs> OnKeyUp;

        public event EventHandler<MouseMoveEventArgs> OnMouseMove;

        public enum MouseButtons
        {
            Left,
            Middle,
            Right,
            Extra1,
            Extra2
        }

        private KeyboardState oldKeyState;
        private MouseState oldMouseState;

        private Point oldMousePosition;

        public InputController(Game game)
            : base(game)
        {
            game.Services.AddService(typeof(InputController), this);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            oldKeyState = Keyboard.GetState();
            oldMouseState = Mouse.GetState();

            oldMousePosition = new Point(oldMouseState.X, oldMouseState.Y);

            base.Initialize();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            UpdateKeyboard();
            UpdateMouse();

            base.Update(gameTime);
        }

        private void UpdateMouse()
        {
            MouseState newState = Mouse.GetState();

            Point position = new Point(newState.X, newState.Y);
            Vector2 distance = new Vector2(oldMousePosition.X - position.X, oldMousePosition.Y - position.Y);

            Mouse.SetPosition(oldMousePosition.X, oldMousePosition.Y);

            if ((distance.LengthSquared() != 0) && (OnMouseMove != null))
            {
                OnMouseMove(null, new MouseMoveEventArgs(distance));
            }
        }

        private void UpdateKeyboard()
        {
            KeyboardState newState = Keyboard.GetState();

            foreach (Keys k in newState.GetPressedKeys())
            {
                if (!oldKeyState.IsKeyDown(k) && (OnKeyDown != null))
                {
                    OnKeyDown(null, new KeyEventArgs(k));
                }
            }

            foreach (Keys k in oldKeyState.GetPressedKeys())
            {
                if (!newState.IsKeyDown(k) && (OnKeyUp != null))
                {
                    OnKeyUp(null, new KeyEventArgs(k));
                }
            }

            oldKeyState = newState;
        }
    }
}