using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace Idyll.SceneGraph
{
    public class MatrixStack
    {
        private Stack<Matrix> stack;

        public MatrixStack()
        {
            stack = new Stack<Matrix>();
        }

        /// <summary>
        /// Gets the current top-of-stack value without removing it.
        /// </summary>
        /// <returns></returns>
        public Matrix Peek() { return stack.Count > 0 ? stack.Peek() : Matrix.Identity; }

        /// <summary>
        /// Adds a new matrix to the top of the stack.
        /// </summary>
        /// <param name="m"></param>
        public void Push(Matrix m) { stack.Push(m); }

        /// <summary>
        /// Removes the value on the top of the stack and returns it.
        /// </summary>
        /// <returns></returns>
        public Matrix Pop() { return stack.Count > 0 ? stack.Pop() : Matrix.Identity; }
    }
}
