using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;

namespace Idyll.SceneGraph
{
    public class SceneNode
    {
        public SceneNode Parent { get; private set; }

        private List<SceneNode> childNodes;
        private bool isInitialised = false;

        public SceneNode()
        {
            childNodes = new List<SceneNode>();
        }

        public void AddNode(SceneNode n)
        {
            n.Parent = this;
            childNodes.Add(n);
            if (isInitialised)
            {
                n.Initialise();
            }
        }

        public void RemoveNode(SceneNode n)
        {
            n.UnloadContent();
            childNodes.Remove(n);
        }

        public virtual void Initialise()
        {
            foreach (SceneNode n in childNodes)
            {
                n.Initialise();
            }
            isInitialised = true;
        }

        public virtual void LoadContent(Scene scene, ContentManager contentManager)
        {
            foreach (SceneNode n in childNodes)
            {
                n.LoadContent(scene, contentManager);
            }
        }

        public virtual void UnloadContent()
        {
            foreach (SceneNode n in childNodes)
            {
                n.UnloadContent();
            }
        }

        public virtual void Update(Scene scene, GameTime gameTime)
        {
            foreach (SceneNode n in childNodes.ToArray())
            {
                n.Update(scene, gameTime);
            }
        }

        public virtual bool PreRender(Scene scene, GraphicsDevice graphicsDevice) { return true; }

        public virtual void Render(Scene scene, GraphicsDevice graphicsDevice) { }

        public virtual void RenderChildren(Scene scene, GraphicsDevice graphicsDevice)
        {
            foreach (SceneNode n in childNodes)
            {
                if (n.PreRender(scene, graphicsDevice))
                {
                    n.Render(scene, graphicsDevice);
                    n.RenderChildren(scene, graphicsDevice);
                    n.PostRender(scene, graphicsDevice);
                }
            }
        }

        public virtual void PostRender(Scene scene, GraphicsDevice graphicsDevice) { }
    }
}
