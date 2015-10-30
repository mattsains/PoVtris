using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    class Tetromino
    {
        public enum Shapes { I, S, Z, O, L, J, T };

        static bool t = true, f = false;
        public static readonly Dictionary<Shapes, bool[,]> Structures = new Dictionary<Shapes, bool[,]>()
        {
             { Shapes.I, new bool[4,4]
                {
                    {f, t, f, f },
                    {f, t, f, f },
                    {f, t, f, f },
                    {f, t, f, f }
                }
             },
            { Shapes.J, new bool[4,4]
                {
                    {f, t, f, f },
                    {f, t, f, f },
                    {t, t, f, f },
                    {f, f, f, f }
                }
              },
            { Shapes.L, new bool[4,4]
                {
                    {f, t, f, f },
                    {f, t, f, f },
                    {f, t, t, f },
                    {f, f, f, f }
                }
            },
            { Shapes.O,new bool[4,4]
                {
                    {f, f, f, f },
                    {f, t, t, f },
                    {f, t, t, f },
                    {f, f, f, f }
                }
            },
            { Shapes.S, new bool[4,4]
                {
                    {f, f, f, f },
                    {f, t, t, f },
                    {t, t, f, f },
                    {f, f, f, f }
                }
            },
            { Shapes.T, new bool[4,4]
                {
                    {f, t, f, f },
                    {t, t, f, f },
                    {f, t, f, f },
                    {f, f, f, f }
                }
            },
            { Shapes.Z, new bool[4,4]
                {
                    {f, f, f, f },
                    {t, t, f, f },
                    {f, t, t, f },
                    {f, f, f, f }
                }
            }
        };

        /// <summary>
        /// Rotates anticlockwise n*90 degrees.
        /// </summary>
        /// <param name="shape">4x4 bool matrix</param>
        /// <returns></returns>
        public static bool[,] Rotate(bool[,] shape, int n)
        {
            if (n == 0)
                return shape;
            else
            {
                bool[,] result = new bool[4, 4];

                if (n == 1)
                {
                    for (int x = 0; x < 4; x++)
                        for (int y = 0; y < 4; y++)
                            result[y, 3 - x] = shape[x, y];
                }
                else if (n == 2)
                {
                    for (int x = 0; x < 4; x++)
                        for (int y = 0; y < 4; y++)
                            result[3 - x, 3 - y] = shape[x, y];
                }
                else if (n == 3)
                {
                    for (int x = 0; x < 4; x++)
                        for (int y = 0; y < 4; y++)
                            result[3 - y, x] = shape[x, y];
                }

                return result;
            }
        }

        public static readonly Dictionary<Shapes, Color> Colors = new Dictionary<Shapes, Color>() 
        { 
            { Shapes.I, Color.Cyan }, 
            { Shapes.J, Color.Blue }, 
            { Shapes.L, Color.Orange }, 
            { Shapes.O, Color.Yellow }, 
            { Shapes.S, Color.Lime },
            { Shapes.T, Color.Purple }, 
            { Shapes.Z, Color.Red } 
        };
        public int X, Y;
        public bool[,] Structure;
        public Color Color;

        private Tetris TetrisState;

        public Tetromino(Tetris tetrisState)
        {
            this.TetrisState = tetrisState;
        }

        public void Spawn(Shapes shape, int x, int y, int rotation)
        {
            this.X = x;
            this.Y = y;
            this.Color = Tetromino.Colors[shape];

            this.Structure = Rotate(Tetromino.Structures[shape], rotation);

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (Structure[i, j])
                        TetrisState.Grid[i + X, j + Y] = Color;

            TetrisState.Tetrominoes.AddLast(this);
        }

        public void Despawn()
        {
            TetrisState.Tetrominoes.Remove(this);
        }

        public static int[] FindBottomSurfaces(bool[,] shape)
        {
            int[] result = new int[4];


            for (int x = 0; x < 4; x++)
            {
                int bottomSurface = 0;
                for (int y = 3; y >= 0; y--)
                {
                    if (shape[x, y])
                    {
                        bottomSurface = y + 1;
                        break;
                    }
                }
                result[x] = bottomSurface;
            }
            return result;
        }

        public static int[] FindLeftSurfaces(bool[,] shape)
        {
            int[] result = new int[4];


            for (int y = 0; y < 4; y++)
            {
                int leftSurface = 4;
                for (int x = 0; x < 4; x++)
                {
                    if (shape[x, y])
                    {
                        leftSurface = x - 1;
                        break;
                    }
                }
                result[y] = leftSurface;
            }
            return result;
        }
        public static int[] FindTopSurfaces(bool[,] shape)
        {
            int[] result = new int[4];


            for (int x = 0; x < 4; x++)
            {
                int topSurface = 4;
                for (int y = 0; y < 4; y++)
                {
                    if (shape[x, y])
                    {
                        topSurface = y - 1;
                        break;
                    }
                }
                result[x] = topSurface;
            }
            return result;
        }

        public static int[] FindRightSurfaces(bool[,] shape)
        {
            int[] result = new int[4];


            for (int y = 0; y < 4; y++)
            {
                int rightSurface = -1;
                for (int x = 3; x >= 0; x--)
                {
                    if (shape[x, y])
                    {
                        rightSurface = x + 1;
                        break;
                    }
                }
                result[y] = rightSurface;
            }
            return result;
        }

        /// <summary>
        /// Moves a piece down the grid
        /// </summary>
        /// <param name="collapse">Whether to move the piece all the way down to its resting point</param>
        /// <returns>Whether The piece is at its resting point</returns>
        public bool MoveDown(bool collapse = false)
        {
            do
            {
                int[] bottoms = FindBottomSurfaces(Structure);

                for (int x = 0; x < 4; x++)
                {

                    if (Y + bottoms[x] == Tetris.Height || (bottoms[x] != 0 && TetrisState.Grid[X + x, Y + bottoms[x]] != Color.TransparentBlack))
                        return true;
                }

                //if we get here then we can move

                for (int x = 0; x < 4; x++)
                    for (int y = 0; y < 4; y++)
                        if (Structure[x, y])
                            TetrisState.Grid[x + X, y + Y] = Color.TransparentBlack;
                Y++;

                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        if (Structure[i, j])
                            TetrisState.Grid[i + X, j + Y] = Color;
            }
            while (collapse);

            return false;
        }

        public void MoveLeft()
        {
            int[] lefts = FindLeftSurfaces(Structure);
            for (int y = 0; y < 4; y++)
                if (lefts[y] != 4 && (lefts[y] + X < 0 || TetrisState.Grid[X + lefts[y], Y + y] != Color.TransparentBlack))
                    return;

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    if (Structure[x, y])
                        TetrisState.Grid[x + X, y + Y] = Color.TransparentBlack;
            X--;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (Structure[i, j])
                        TetrisState.Grid[i + X, j + Y] = Color;
        }

        public void MoveRight()
        {
            int[] rights = FindRightSurfaces(Structure);
            for (int y = 0; y < 4; y++)
                if (rights[y] != -1 && (rights[y] + X >= Tetris.Width || TetrisState.Grid[X + rights[y], Y + y] != Color.TransparentBlack))
                    return;

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    if (Structure[x, y])
                        TetrisState.Grid[x + X, y + Y] = Color.TransparentBlack;
            X++;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (Structure[i, j])
                        TetrisState.Grid[i + X, j + Y] = Color;
        }

        public bool Rotate(bool left)
        {
            int oldX = X, oldY = Y;
            //erase old location
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    if (Structure[x, y])
                        TetrisState.Grid[x + X, y + Y] = Color.TransparentBlack;

            bool[,] candidateStructure = Rotate(Structure, left ? 1 : 3);



            //check wall collisions, and try to correct for them
            //is there a left collision?
            int leftBound = FindLeftSurfaces(candidateStructure).Min();
            if (X + leftBound < 0)
                X = -leftBound - 1;
            int rightBound = FindRightSurfaces(candidateStructure).Max();
            if (X + rightBound >= Tetris.Width)
                X = Tetris.Width - rightBound;
            int bottomBound = FindBottomSurfaces(candidateStructure).Max();
            if (Y + bottomBound >= Tetris.Height)
                Y = Tetris.Height - bottomBound;

            //check block intersections
            bool failure = false;
            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (candidateStructure[i, j] && TetrisState.Grid[i + X, j + Y] != Color.TransparentBlack)
                    {
                        failure = true;
                        break;
                    }

            if (!failure)
            {
                //we have a valid location, put the block there
                Structure = candidateStructure;
                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        if (Structure[i, j])
                            TetrisState.Grid[i + X, j + Y] = Color;
                return true;
            }
            else
            {
                X = oldX;
                Y = oldY;
                for (int x = 0; x < 4; x++)
                    for (int y = 0; y < 4; y++)
                        if (Structure[x, y])
                            TetrisState.Grid[x + X, y + Y] = Color;
                return false;
            }
        }
    }

    class Tetris
    {
        public const int Height = 22;
        public const int Width = 10;

        Random r = new Random();

        public Color[,] Grid = new Color[Width, Height];

        public void Initialize(GraphicsDevice graphicsDevice)
        {
            VertexBuffer = new DynamicVertexBuffer(graphicsDevice, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);
            IndexBuffer = new DynamicIndexBuffer(graphicsDevice, typeof(ushort), 36, BufferUsage.WriteOnly);
        }

        DynamicVertexBuffer VertexBuffer;
        DynamicIndexBuffer IndexBuffer;

        public Tetromino current = null;
        public LinkedList<Tetromino> Tetrominoes = new LinkedList<Tetromino>();

        public bool Update()
        {
            bool gameOver = false;
            if (current == null)
            {
                current = new Tetromino(this);
                List<Tetromino.Shapes> shapeNames = Tetromino.Structures.Keys.ToList();
                Tetromino.Shapes newShape = shapeNames[r.Next(shapeNames.Count)];
                current.Spawn(newShape, 3, 0, r.Next(4));
            }
            else
            {
                bool done = current.MoveDown();
                if (done)
                {
                    for (int i = 0; i < 3; i++)
                        for (int j = 0; j < Width; j++)
                            if (Grid[j, i] != Color.TransparentBlack)
                                gameOver = true;

                    current.Despawn();

                    ClearFullLines();

                    current = new Tetromino(this);
                    List<Tetromino.Shapes> shapeNames = Tetromino.Structures.Keys.ToList();
                    Tetromino.Shapes newShape = shapeNames[r.Next(shapeNames.Count)];
                    current.Spawn(newShape, 3, 0, r.Next(4));
                }
            }
            return gameOver;
        }

        public void Draw(BasicEffect basicEffect)
        {
            Dictionary<Color, Color> OutlineColours = new Dictionary<Color, Color>() 
                { 
                    { Color.Cyan,Color.DarkCyan }, 
                    { Color.Blue ,Color.DarkBlue}, 
                    {  Color.Orange,Color.DarkOrange }, 
                    {  Color.Yellow ,Color.YellowGreen}, 
                    { Color.Lime ,Color.Green},
                    { Color.Purple ,Color.Indigo}, 
                    {  Color.Red ,Color.DarkRed} 
                };

            GraphicsDevice device = basicEffect.GraphicsDevice;


            for (int x = 0; x <= Grid.GetUpperBound(0); x++)
            {
                for (int y = 0; y <= Grid.GetUpperBound(1); y++)
                {
                    if (Grid[x, y] != Color.TransparentBlack)
                    {
                        VertexPositionColor[] square = new VertexPositionColor[8];

                        Color fill = Grid[x, y];
                        Color outline = OutlineColours[fill];

                        const float borderWidth = 0.05f;

                        square[0] = new VertexPositionColor(new Vector3(x, y, 0), outline);
                        square[1] = new VertexPositionColor(new Vector3(x + 1, y, 0), outline);
                        square[2] = new VertexPositionColor(new Vector3(x + 1, y + 1, 0), outline);
                        square[3] = new VertexPositionColor(new Vector3(x, y + 1, 0), outline);

                        square[4] = new VertexPositionColor(new Vector3(x + borderWidth, y + borderWidth, 0), fill);
                        square[5] = new VertexPositionColor(new Vector3(x + 1 - borderWidth, y + borderWidth, 0), fill);
                        square[6] = new VertexPositionColor(new Vector3(x + 1 - borderWidth, y + 1 - borderWidth, 0), fill);
                        square[7] = new VertexPositionColor(new Vector3(x + borderWidth, y + 1 - borderWidth, 0), fill);

                        short[] index = new short[] { 0, 1, 3, 2 };

                        foreach (EffectPass pass in basicEffect.CurrentTechnique.Passes)
                        {
                            pass.Apply();

                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, square, 0, 4, index, 0, 2); //outline
                            device.DrawUserIndexedPrimitives(PrimitiveType.TriangleStrip, square, 4, 4, index, 0, 2); //fill
                        }

                    }
                }
            }
        }

        public bool[] FindFullLines()
        {
            bool[] result = new bool[Height];

            for (int y = 0; y < Height; y++)
            {
                bool full = true;
                for (int x = 0; x < Width; x++)
                {
                    if (Grid[x, y] == Color.TransparentBlack)
                    {
                        full = false;
                        break;
                    }
                }
                result[y] = full;
            }
            return result;
        }

        public int[] ClearFullLines()
        {
            bool[] toClear = FindFullLines();

            int lookingAtY = Height - 1;
            int copyFromY = Height - 1;

            while (copyFromY > 0)
            {
                while (toClear[copyFromY])
                    copyFromY--;
                if (copyFromY == 0)
                    break;

                if (lookingAtY != copyFromY)
                    for (int x = 0; x < Width; x++)
                        Grid[x, lookingAtY] = Grid[x, copyFromY];
                copyFromY--;
                lookingAtY--;
            }
            while (lookingAtY >= 0)
            {
                for (int i = 0; i < Width; i++)
                    Grid[i, lookingAtY] = Color.TransparentBlack;
                lookingAtY--;
            }
            return new int[1];
        }
    }
}
