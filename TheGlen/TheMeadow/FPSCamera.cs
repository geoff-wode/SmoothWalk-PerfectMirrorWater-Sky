using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;

using Idyll.Input;
using Idyll.SceneGraph;

namespace Meadow
{
    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class FPSCamera : Microsoft.Xna.Framework.DrawableGameComponent, ICamera
    {
        protected Vector3 position;
        protected Vector3 target;
        protected Vector3 up;
        protected Vector3 right;

        private Matrix view;
        private Matrix projection;

        protected const float rotationSpeed = 0.1f;
        protected const float movementSpeed = 0.1f;

        protected float yaw = 0;
        protected float pitch = 0;

        protected bool goForward = false;
        protected bool goBackward = false;
        protected bool strafeLeft = false;
        protected bool strafeRight = false;

        protected delegate void KeyAction();
        protected Dictionary<Keys, KeyAction> keyDownActions;
        protected Dictionary<Keys, KeyAction> keyUpActions;

        public FPSCamera(Game game)
            : base(game)
        {
            position = Vector3.Zero;
            target = Vector3.Forward;
            up = Vector3.Up;
            right = Vector3.Right;

            keyDownActions = new Dictionary<Keys, KeyAction>();
            keyUpActions = new Dictionary<Keys, KeyAction>();

            game.Services.AddService(typeof(ICamera), this);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            keyDownActions[Keys.W] = delegate() { goForward = true; };
            keyDownActions[Keys.A] = delegate() { strafeLeft = true; };
            keyDownActions[Keys.D] = delegate() { strafeRight = true; };
            keyDownActions[Keys.S] = delegate() { goBackward = true; };

            keyUpActions[Keys.W] = delegate() { goForward = false; };
            keyUpActions[Keys.A] = delegate() { strafeLeft = false; };
            keyUpActions[Keys.D] = delegate() { strafeRight = false; };
            keyUpActions[Keys.S] = delegate() { goBackward = false; };

            InputController input = (InputController)Game.Services.GetService(typeof(InputController));

            input.OnKeyDown += new EventHandler<KeyEventArgs>(input_OnKeyDown);
            input.OnKeyUp += new EventHandler<KeyEventArgs>(input_OnKeyUp);
            input.OnMouseMove += new EventHandler<MouseMoveEventArgs>(input_OnMouseMove);

            base.Initialize();
        }

        protected override void LoadContent()
        {
            float aspectRatio = (float)GraphicsDevice.Viewport.Width / (float)GraphicsDevice.Viewport.Height;

            projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.ToRadians(60), aspectRatio, 1, 1000);

            base.LoadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            // Decide if the position needs to be updated...
            Vector3 motion = Vector3.Zero;

            if (goForward) { motion += Vector3.Forward; }
            if (goBackward) { motion += Vector3.Backward; }
            if (strafeLeft) { motion += Vector3.Left; }
            if (strafeRight) { motion += Vector3.Right; }

            Matrix rotationY = Matrix.CreateRotationY(MathHelper.ToRadians(yaw));
            Matrix rotationX = Matrix.CreateRotationX(MathHelper.ToRadians(pitch));
            Matrix rotationXY = rotationX * rotationY;

            up = Vector3.Transform(Vector3.Up, rotationXY);
            motion = Vector3.Transform(motion, rotationXY);

            position += motion * movementSpeed;

            target = Vector3.Transform(Vector3.Forward, rotationXY);
            right = Vector3.Transform(Vector3.Right, rotationXY);

            Geology.ITerrainInfo terrain = (Geology.ITerrainInfo)Game.Services.GetService(typeof(Geology.ITerrainInfo));

            float height = terrain.GetHeightAt(position.X, position.Z) + 1.8f;

            position.Y = height;

            view = Matrix.CreateLookAt(position, target + position, up);

            base.Update(gameTime);
        }

        private void input_OnMouseMove(object sender, MouseMoveEventArgs e)
        {
            yaw += rotationSpeed * e.Distance.X;
            pitch += rotationSpeed * e.Distance.Y;
            pitch = MathHelper.Clamp(pitch, -75, 89);
        }

        private void input_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (keyUpActions.ContainsKey(e.Key))
            {
                keyUpActions[e.Key]();
            }
        }

        private void input_OnKeyDown(object sender, KeyEventArgs e)
        {
            if (keyDownActions.ContainsKey(e.Key))
            {
                keyDownActions[e.Key]();
            }
        }

        #region ICamera Members

        public Matrix View
        {
            get { return view; }
        }

        public Matrix Projection
        {
            get { return projection; }
        }

        public Vector3 Position
        {
            get { return position; }
            set { position = value; }
        }

        public Vector3 Target
        {
            get { return target; }
        }

        public Vector3 Up
        {
            get { return up; }
        }

        public Vector3 Right
        {
            get { return right; }
        }

        #endregion
    }
}