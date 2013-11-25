using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;

using Idyll;
using Idyll.SceneGraph;

namespace Meadow.Geology
{
    public interface ITerrainInfo
    {
        int MapWidth { get; }
        int MapLength { get; }

        float GetHeightAt(float x, float z);
    }

    public class Terrain : SceneNode, ITerrainInfo
    {
        private struct VertexPositionNormalMultiTexture
        {
            public Vector3 Position;
            public Vector3 Normal;
            public Vector2 TextureCoordinate;
            public Vector4 TexWeights;

            public static int SizeInBytes = (3 + 3 + 2 + 4) * sizeof(float);
            public static VertexElement[] VertexElements = new VertexElement[]
            {
                new VertexElement( 0, 0, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Position, 0 ),
                new VertexElement( 0, sizeof(float) * 3, VertexElementFormat.Vector3, VertexElementMethod.Default, VertexElementUsage.Normal, 0 ),
                new VertexElement( 0, sizeof(float) * 6, VertexElementFormat.Vector2, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 0 ),
                new VertexElement( 0, sizeof(float) * 8, VertexElementFormat.Vector4, VertexElementMethod.Default, VertexElementUsage.TextureCoordinate, 1 )
            };
        }

        private VertexDeclaration vertexDecl;
        private VertexBuffer vb;
        private IndexBuffer ib;

        private Grid<VertexPositionNormalMultiTexture> grid;
        private float[,] heightMap;
        private Texture2D heightTexture;
        private Texture2D[] textures;
        private Effect effect;

        private Matrix world;

        private readonly string[] textureNames = { "Sand", "Grass", "Rock", "Snow" };

        public override void LoadContent(Scene scene, ContentManager contentManager)
        {
            scene.Game.Services.AddService(typeof(ITerrainInfo), this);

            vertexDecl = new VertexDeclaration(scene.Game.GraphicsDevice, VertexPositionNormalMultiTexture.VertexElements);
            effect = contentManager.Load<Effect>("Effects/Terrain");

            textures = new Texture2D[4];
            for (int i = 0; i < 4; i++)
            {
                textures[i] = contentManager.Load<Texture2D>("Textures/Terrain/" + textureNames[i]);
            }

            heightTexture = contentManager.Load<Texture2D>("Textures/Terrain/heightMap");
            grid = new Grid<VertexPositionNormalMultiTexture>(heightTexture.Width, heightTexture.Height);
            LoadHeightMap();
            grid.DefineGrid(PopulateVertex);
            DefineNormals();

            vb = new VertexBuffer(scene.Game.GraphicsDevice, typeof(VertexPositionNormalMultiTexture), grid.Vertices.Length, BufferUsage.WriteOnly);
            vb.SetData<VertexPositionNormalMultiTexture>(grid.Vertices);

            ib = new IndexBuffer(scene.Game.GraphicsDevice, typeof(int), grid.Indices.Length, BufferUsage.WriteOnly);
            ib.SetData<int>(grid.Indices);

            world = Matrix.CreateTranslation(-grid.Width / 2, 0, grid.Height / 2);

            base.LoadContent(scene, contentManager);
        }

        public override void UnloadContent()
        {
            vertexDecl.Dispose();
            vb.Dispose();
            ib.Dispose();

            base.UnloadContent();
        }

        public override void Render(Scene scene, GraphicsDevice graphicsDevice)
        {
            GraphicsDevice device = scene.Game.GraphicsDevice;

            device.Vertices[0].SetSource(vb, 0, VertexPositionNormalMultiTexture.SizeInBytes);
            device.Indices = ib;
            device.VertexDeclaration = vertexDecl;

            effect.Parameters["World"].SetValue(world * scene.MatrixStack.Peek());

            Weather.Sun sun = (Weather.Sun)scene.Game.Services.GetService(typeof(Weather.Sun));
            effect.Parameters["LightDirection"].SetValue(sun.LightDirection);

            for (int i = 0; i < 4; i++)
            {
                effect.Parameters[textureNames[i] + "Texture"].SetValue(textures[i]);
            }

            effect.CurrentTechnique = effect.Techniques["MultiTextured"];
            effect.Begin();
            foreach (EffectPass pass in effect.CurrentTechnique.Passes)
            {
                pass.Begin();
                device.DrawIndexedPrimitives(PrimitiveType.TriangleStrip, 0, 0, grid.Vertices.Length, 0, grid.PrimitiveCount);
                pass.End();
            }
            effect.End();

            base.Render(scene, graphicsDevice);
        }

        private void PopulateVertex(int x, int z, ref VertexPositionNormalMultiTexture v)
        {
            v.Position = new Vector3(x, heightMap[x, z], -z);
            v.TextureCoordinate = new Vector2((float)x / 30, (float)z / 30);

            const float sandMiddle = 0;
            const float sandRange = 10;
            const float grassMiddle = 20;
            const float grassRange = 14;
            const float rockMiddle = 38;
            const float rockRange = 10;
            const float snowMiddle = 50;
            const float snowRange = 6;

            float h = heightMap[x, z];
            Vector4 weight = new Vector4();
            weight.X = MathHelper.Clamp(1.0f - Math.Abs(h - sandMiddle) / sandRange, 0, 1);
            weight.Y = MathHelper.Clamp(1.0f - Math.Abs(h - grassMiddle) / grassRange, 0, 1);
            weight.Z = MathHelper.Clamp(1.0f - Math.Abs(h - rockMiddle) / rockRange, 0, 1);
            weight.W = MathHelper.Clamp(1.0f - Math.Abs(h - snowMiddle) / snowRange, 0, 1);

            float total = weight.X + weight.Y + weight.Z + weight.W;

            v.TexWeights = weight / total;
        }

        private void DefineNormals()
        {
            for (int i = 0; i < grid.Vertices.Length; i++)
            {
                grid.Vertices[i].Normal = new Vector3(0, 0, 0);
            }

            // Triangles strips use alternating winding order, so fix up the normals as needed...
            float winding = 1;

            for (int i = 2; i < grid.Indices.Length; i++)
            {
                Vector3 v1 = grid.Vertices[grid.Indices[i - 1]].Position - grid.Vertices[grid.Indices[i]].Position;
                Vector3 v2 = grid.Vertices[grid.Indices[i - 2]].Position - grid.Vertices[grid.Indices[i]].Position;

                Vector3 normal = Vector3.Cross(v1, v2);
                normal.Normalize();

                normal *= winding;

                if (!float.IsNaN(normal.X))
                {
                    grid.Vertices[grid.Indices[i]].Normal += normal;
                    grid.Vertices[grid.Indices[i - 1]].Normal += normal;
                    grid.Vertices[grid.Indices[i - 2]].Normal += normal;
                }

                winding *= -1;
            }

            for (int i = 0; i < grid.Vertices.Length; i++)
            {
                grid.Vertices[i].Normal.Normalize();
            }
        }

        private void LoadHeightMap()
        {
            int w = heightTexture.Width;
            int h = heightTexture.Height;

            Color[] colours = new Color[w * h];
            heightTexture.GetData<Color>(colours);

            float maxHeight = float.MinValue;
            float minHeight = float.MaxValue;

            heightMap = new float[w, h];

            for (int z = 0; z < h; z++)
            {
                for (int x = 0; x < w; x++)
                {
                    float height = colours[x + (z * w)].R / 5f;
                    heightMap[x, z] = height;

                    maxHeight = MathHelper.Max(maxHeight, height);
                    minHeight = MathHelper.Min(minHeight, height);
                }
            }

            float range = maxHeight - minHeight;
            for (int z = 0; z < h; z++)
            {
                for (int x = 0; x < w; x++)
                {
                    heightMap[x, z] = ((heightMap[x, z] - minHeight) / range) * 40;
                }
            }
        }

        #region ITerrainInfo Members

        int ITerrainInfo.MapWidth
        {
            get { return grid.Width; }
        }

        int ITerrainInfo.MapLength
        {
            get { return grid.Height; }
        }

        public float GetHeightAt(float x, float z)
        {
            // Put the coordinates into the map and clamp it to range.
            x = MathHelper.Clamp(x + grid.Width / 2, 0, grid.Width);
            z = MathHelper.Clamp(z + grid.Height / 2, 0, grid.Height);

            //return heightMap[(int)Math.Abs(x) % grid.Width, (int)Math.Abs(z) % grid.Height];
            return GetExactHeight(x, z);
        }

        #endregion

        private float GetExactHeight(float x, float z)
        {
            // Find the integer coordinates within which the passed values are located...
            int lowerX = (int)x;
            int upperX = lowerX + 1;
            float relativeX = (x - lowerX) / (float)(upperX - lowerX);

            int lowerZ = (int)z;
            int upperZ = lowerZ + 1;
            float relativeZ = (z - lowerZ) / (float)(upperZ - lowerZ);

            // Get the height at each "corner"...
            float bottomLeft = heightMap[lowerX, lowerZ];
            float topLeft = heightMap[lowerX, upperZ];
            float topRight = heightMap[upperX, upperZ];
            float bottomRight = heightMap[upperX, lowerZ];

            // If relX + relZ < 1, the coordinate must be inside the bottom left of the
            // 2 triangles that define the whole square...

            bool inUpperLeftTriangle = (relativeX + relativeZ) < 1;

            float height = 0;
            if (inUpperLeftTriangle)
            {
                height = bottomLeft;
                height += relativeZ * (topLeft - bottomLeft);
                height += relativeX * (bottomRight - bottomLeft);
            }
            else
            {
                height = topRight;
                height += (1 - relativeZ) * (bottomRight - topRight);
                height += (1 - relativeX) * (topLeft - topRight);
            }

            return height;
        }
    }
}
