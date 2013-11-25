using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Idyll;
using Idyll.SceneGraph;

namespace Meadow.Weather
{
    public class SkyDome : SceneNode
    {
        private Model skyDome;
        private Texture2D cloudTexture;
        private Matrix[] bones;
        private Matrix world;

        public override void LoadContent(Scene scene, ContentManager contentManager)
        {
            cloudTexture = contentManager.Load<Texture2D>("Textures/Weather/clouds");
            Effect effect = contentManager.Load<Effect>("Effects/Textured");

            skyDome = contentManager.Load<Model>("Models/skydome");

            bones = new Matrix[skyDome.Bones.Count];
            skyDome.CopyAbsoluteBoneTransformsTo(bones);

            skyDome.Meshes[0].MeshParts[0].Effect = effect.Clone(scene.GraphicsDevice);

            world = Matrix.CreateTranslation(0, -0.2f, 0) * Matrix.CreateScale(100);

            base.LoadContent(scene, contentManager);
        }

        public override void Render(Scene scene, GraphicsDevice graphicsDevice)
        {
            ICamera camera = (ICamera)scene.Game.Services.GetService(typeof(ICamera));

            scene.GraphicsDevice.RenderState.DepthBufferWriteEnable = false;

            Matrix transform = world * Matrix.CreateTranslation(camera.Position);

            foreach (ModelMesh mesh in skyDome.Meshes)
            {
                foreach (Effect effect in mesh.Effects)
                {
                    Matrix worldMatrix = bones[mesh.ParentBone.Index] * transform;
                    effect.CurrentTechnique = effect.Techniques["Textured"];
                    effect.Parameters["World"].SetValue(worldMatrix);

                    effect.Parameters["Texture"].SetValue(cloudTexture);
                }
                mesh.Draw();
            }
            scene.GraphicsDevice.RenderState.DepthBufferWriteEnable = true;

            base.Render(scene, graphicsDevice);
        }
    }
}
