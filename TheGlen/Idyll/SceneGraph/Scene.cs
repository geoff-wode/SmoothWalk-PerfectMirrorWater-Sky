using System;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
using Microsoft.Xna.Framework.Net;
using Microsoft.Xna.Framework.Storage;

using Idyll.Input;

namespace Idyll.SceneGraph
{
    public interface ICamera
    {
        Matrix View { get; }
        Matrix Projection { get; }

        Vector3 Position { get; set; }
        Vector3 Target { get; }
        Vector3 Up { get; }
        Vector3 Right { get; }
    }

    public interface IScenePreProcessor
    {
        void PreProcess(Scene scene);
    }
    public interface IScenePostProcessor
    {
        void PostProcess(Scene scene);
    }

    /// <summary>
    /// This is a game component that implements IUpdateable.
    /// </summary>
    public class Scene : Microsoft.Xna.Framework.DrawableGameComponent
    {
        public MatrixStack MatrixStack { get; private set; }

        public SceneNode RootNode { get; private set; }

        public SpriteBatch SpriteBatch { get; private set; }

        public Effect EffectCommonParameters { get; private set; }

        private List<IScenePreProcessor> preProcessors;
        private List<IScenePostProcessor> postProcessors;

        public Scene(Game game)
            : base(game)
        {
            preProcessors = new List<IScenePreProcessor>();
            postProcessors = new List<IScenePostProcessor>();

            RootNode = new SceneNode();

            game.Services.AddService(typeof(Scene), this);
        }

        /// <summary>
        /// Allows the game component to perform any initialization it needs to before starting
        /// to run.  This is where it can query for any required services and load content.
        /// </summary>
        public override void Initialize()
        {
            RootNode.Initialise();

            MatrixStack = new MatrixStack();

            base.Initialize();
        }

        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);

            EffectCommonParameters = Game.Content.Load<Effect>("Effects/CommonParameters");

            RootNode.LoadContent(this, Game.Content);

            base.LoadContent();
        }

        protected override void UnloadContent()
        {
            RootNode.UnloadContent();

            base.UnloadContent();
        }

        /// <summary>
        /// Allows the game component to update itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        public override void Update(GameTime gameTime)
        {
            RootNode.Update(this, gameTime);

            base.Update(gameTime);
        }

        public override void Draw(GameTime gameTime)
        {
            foreach (IScenePreProcessor p in preProcessors)
            {
                p.PreProcess(this);
            }

            ICamera camera = (ICamera)Game.Services.GetService(typeof(ICamera));
            EffectCommonParameters.Parameters["View"].SetValue(camera.View);
            EffectCommonParameters.Parameters["Projection"].SetValue(camera.Projection);

            RenderScene();

            foreach (IScenePostProcessor p in postProcessors)
            {
                p.PostProcess(this);
            }

            base.Draw(gameTime);
        }

        public void RenderScene()
        {
            GraphicsDevice.Clear(ClearOptions.DepthBuffer | ClearOptions.Target, Color.LightSkyBlue, 1, 0);
            if (RootNode.PreRender(this, Game.GraphicsDevice))
            {
                RootNode.Render(this, Game.GraphicsDevice);
                RootNode.RenderChildren(this, Game.GraphicsDevice);
                RootNode.PostRender(this, Game.GraphicsDevice);
            }
        }

        public void AddPreProcessor(IScenePreProcessor p)
        {
            preProcessors.Add(p);
        }

        public void RemovePreProcessor(IScenePreProcessor p)
        {
            preProcessors.Remove(p);
        }

        public void AddPostProcessor(IScenePostProcessor p)
        {
            postProcessors.Add(p);
        }

        public void RemovePostProcessor(IScenePostProcessor p)
        {
            postProcessors.Remove(p);
        }
    }
}