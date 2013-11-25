using System;

using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

using Idyll;
using Idyll.Input;
using Idyll.SceneGraph;

namespace Meadow
{
    static class Program
    {
        static TheGame game;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        static void Main(string[] args)
        {
            using (game = new TheGame())
            {
                InputController input = new InputController(game);
                game.Components.Add(input);

                input.OnKeyUp += new EventHandler<KeyEventArgs>(input_OnKeyUp);

                FPSCamera camera = new FPSCamera(game);
                game.Components.Add(camera);
                camera.Position = new Vector3(156, 0, 156);

                Scene scene = new Scene(game);
                game.Components.Add(scene);

                Weather.Sun sun = new Weather.Sun(game);
                game.Components.Add(sun);

                scene.RootNode.AddNode(new Weather.SkyDome());
                scene.RootNode.AddNode(new Geology.Terrain());

                Geology.Water water = new Meadow.Geology.Water();
                scene.RootNode.AddNode(water);
                scene.AddPreProcessor(water);
                scene.AddPostProcessor(water);

                game.Run();
            }
        }

        static void input_OnKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Microsoft.Xna.Framework.Input.Keys.Escape)
            {
                game.Exit();
            }
        }
    }
}
