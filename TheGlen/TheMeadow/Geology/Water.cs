using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Idyll;
using Idyll.SceneGraph;

namespace Meadow.Geology
{
    public class Water : SceneNode, IScenePreProcessor, IScenePostProcessor
    {
        private RenderTarget2D reflectionRT;
        private Texture2D texReflection;
        private RenderTarget2D refractionRT;
        private Texture2D texRefraction;
        private VertexPositionTexture[] vertices;
        private VertexDeclaration vertexDecl;
        private Effect effect;
        private Matrix mirrorView;
        private float waterLevel = 7;
        private Matrix transform;

        public override void LoadContent(Scene scene, ContentManager contentManager)
        {
            PresentationParameters pp = scene.GraphicsDevice.PresentationParameters;

            reflectionRT = new RenderTarget2D(scene.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, scene.GraphicsDevice.DisplayMode.Format);
            refractionRT = new RenderTarget2D(scene.GraphicsDevice, pp.BackBufferWidth, pp.BackBufferHeight, 1, scene.GraphicsDevice.DisplayMode.Format);

            ITerrainInfo terrain = (ITerrainInfo)scene.Game.Services.GetService(typeof(ITerrainInfo));

            int w = terrain.MapWidth;
            int h = terrain.MapLength;

            transform = Matrix.CreateTranslation(-w / 2, 0, h / 2);

            vertices = new VertexPositionTexture[4];
            vertices[0] = new VertexPositionTexture(new Vector3(0, waterLevel, 0), new Vector2(0, 1));
            vertices[1] = new VertexPositionTexture(new Vector3(0, waterLevel, -h), new Vector2(0, 0));
            vertices[2] = new VertexPositionTexture(new Vector3(w, waterLevel, 0), new Vector2(1, 1));
            vertices[3] = new VertexPositionTexture(new Vector3(w, waterLevel, -h), new Vector2(1, 0));

            vertexDecl = new VertexDeclaration(scene.GraphicsDevice, VertexPositionTexture.VertexElements);

            effect = contentManager.Load<Effect>("Effects/Water");

            base.LoadContent(scene, contentManager);
        }

        public override void UnloadContent()
        {
            vertexDecl.Dispose();
            vertexDecl = null;

            base.UnloadContent();
        }

        #region IScenePreProcessor Members

        public void PreProcess(Scene scene)
        {
            ICamera camera = (ICamera)scene.Game.Services.GetService(typeof(ICamera));

            RenderReflection(scene, camera);

            RenderRefraction(scene, camera);

        }

        private void RenderReflection(Scene scene, ICamera camera)
        {
            Vector3 p = camera.Position;
            p.Y = -camera.Position.Y + 2 * waterLevel;

            Vector3 t = camera.Target + camera.Position;
            t.Y = -t.Y + 2 * waterLevel;

            Vector3 u = Vector3.Cross(camera.Right, t - p);
            mirrorView = Matrix.CreateLookAt(p, t, u);

            Plane clipPlane = CreatePlane(waterLevel, Vector3.Down, mirrorView, camera.Projection, true);

            scene.EffectCommonParameters.Parameters["View"].SetValue(mirrorView);
            scene.EffectCommonParameters.Parameters["Projection"].SetValue(camera.Projection);

            scene.GraphicsDevice.SetRenderTarget(0, reflectionRT);

            scene.GraphicsDevice.ClipPlanes[0].IsEnabled = true;
            scene.GraphicsDevice.ClipPlanes[0].Plane = clipPlane;

            scene.RenderScene();

            scene.GraphicsDevice.ClipPlanes[0].IsEnabled = false;

            scene.GraphicsDevice.SetRenderTarget(0, null);
            texReflection = reflectionRT.GetTexture();
        }

        private void RenderRefraction(Scene scene, ICamera camera)
        {
            Plane clipPlane = CreatePlane(waterLevel, Vector3.Down, camera.View, camera.Projection, false);

            scene.EffectCommonParameters.Parameters["View"].SetValue(camera.View);
            scene.EffectCommonParameters.Parameters["Projection"].SetValue(camera.Projection);

            scene.GraphicsDevice.ClipPlanes[0].IsEnabled = true;
            scene.GraphicsDevice.ClipPlanes[0].Plane = clipPlane;

            scene.GraphicsDevice.SetRenderTarget(0, refractionRT);

            scene.RenderScene();

            scene.GraphicsDevice.SetRenderTarget(0, null);
            texRefraction = refractionRT.GetTexture();

            scene.GraphicsDevice.ClipPlanes[0].IsEnabled = false;
        }

        #endregion

        #region IScenePostProcessor Members

        public void PostProcess(Scene scene)
        {
            scene.GraphicsDevice.VertexDeclaration = vertexDecl;

            ICamera camera = (ICamera)scene.Game.Services.GetService(typeof(ICamera));
            scene.EffectCommonParameters.Parameters["View"].SetValue(camera.View);
            scene.EffectCommonParameters.Parameters["Projection"].SetValue(camera.Projection);

            effect.CurrentTechnique = effect.Techniques["Water"];

            effect.Parameters["World"].SetValue(transform);

            effect.Parameters["MirrorView"].SetValue(mirrorView);
            effect.Parameters["ReflectionTexture"].SetValue(texReflection);
            effect.Parameters["RefractionTexture"].SetValue(texRefraction);

            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                scene.GraphicsDevice.DrawUserPrimitives<VertexPositionTexture>(PrimitiveType.TriangleStrip, vertices, 0, 2);
                pass.End();
            }
            effect.End();
        }

        #endregion

        private Plane CreatePlane(float height, Vector3 normal, Matrix view, Matrix projection, bool reflect)
        {
            normal.Normalize();
            Vector4 planeCoeffs = new Vector4(normal, height);
            if (reflect)
                planeCoeffs *= -1;

            Matrix worldViewProjection = view * projection;
            Matrix inverseWorldViewProjection = Matrix.Invert(worldViewProjection);
            inverseWorldViewProjection = Matrix.Transpose(inverseWorldViewProjection);

            planeCoeffs = Vector4.Transform(planeCoeffs, inverseWorldViewProjection);
            Plane finalPlane = new Plane(planeCoeffs);

            return finalPlane;
        }
    }
}
