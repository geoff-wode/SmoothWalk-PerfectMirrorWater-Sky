
namespace Idyll
{
    /// <summary>
    /// A regular grid of points represented by an array of arbitrary vertex type.
    /// </summary>
    /// <typeparam name="T">The type of vertex to be contained by the grid.</typeparam>
    public class Grid<T>
    {
        /// <summary>
        /// Callback used to populate the content of a single vertex.
        /// </summary>
        /// <param name="x">X coordinate on the grid.</param>
        /// <param name="z">Z coordinate on the grid.</param>
        /// <param name="vertex">A reference to the vertex.</param>
        public delegate void PopulateVertexDelegate(int x, int z, ref T vertex);

        public int Width { get; private set; }
        public int Height { get; private set; }
        public T[] Vertices { get; private set; }
        public int[] Indices { get; private set; }
        public int PrimitiveCount { get; private set; }

        public Grid(int width, int height)
        {
            Width = width;
            Height = height;
            PrimitiveCount = (Width * 2 * (Height - 1)) - 2;
        }

        public void DefineGrid(PopulateVertexDelegate callback)
        {
            DefineIndices();

            Vertices = new T[Width * Height];

            int i = 0;

            for (int z = 0; z < Height; z++)
            {
                for (int x = 0; x < Width; x++)
                {
                    callback(x, z, ref Vertices[i++]);
                }
            }
        }

        private void DefineIndices()
        {
            Indices = new int[Width * 2 * (Height - 1)];

            int i = 0;
            int z = 0;

            while (z < Height - 1)
            {
                for (int x = 0; x < Width; x++)
                {
                    Indices[i++] = x + (z * Width);
                    Indices[i++] = x + ((z + 1) * Width);
                }

                z++;

                if (z < Height - 1)
                {
                    for (int x = Width - 1; x >= 0; x--)
                    {
                        Indices[i++] = x + ((z + 1) * Width);
                        Indices[i++] = x + (z * Width);
                    }
                }
                z++;
            }
        }
    }
}
