using Microsoft.Xna.Framework;
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

        const float TransitionTime = 0.1f;

        const bool t = true, f = false;
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

                    if (Y + bottoms[x] == Tetris.Height || (bottoms[x] != 0 && TetrisState.Grid[X + x, Y + bottoms[x]] != Color.Transparent))
                        return true;
                }

                //if we get here then we can move

                for (int x = 0; x < 4; x++)
                    for (int y = 0; y < 4; y++)
                        if (Structure[x, y])
                            TetrisState.Grid[x + X, y + Y] = Color.Transparent;
                Y++;

                for (int i = 0; i < 4; i++)
                    for (int j = 0; j < 4; j++)
                        if (Structure[i, j])
                            TetrisState.Grid[i + X, j + Y] = Color;

                TranslationTransition.FromTetromino(TetrisState, this, X, Y - 1, TransitionTime);
            }
            while (collapse);

            return false;
        }

        public void MoveLeft()
        {
            int[] lefts = FindLeftSurfaces(Structure);
            for (int y = 0; y < 4; y++)
                if (lefts[y] != 4 && (lefts[y] + X < 0 || TetrisState.Grid[X + lefts[y], Y + y] != Color.Transparent))
                    return;

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    if (Structure[x, y])
                        TetrisState.Grid[x + X, y + Y] = Color.Transparent;
            X--;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (Structure[i, j])
                        TetrisState.Grid[i + X, j + Y] = Color;

            TranslationTransition.FromTetromino(TetrisState, this, X + 1, Y, TransitionTime);
        }

        public void MoveRight()
        {
            int[] rights = FindRightSurfaces(Structure);
            for (int y = 0; y < 4; y++)
                if (rights[y] != -1 && (rights[y] + X >= Tetris.Width || TetrisState.Grid[X + rights[y], Y + y] != Color.Transparent))
                    return;

            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    if (Structure[x, y])
                        TetrisState.Grid[x + X, y + Y] = Color.Transparent;
            X++;

            for (int i = 0; i < 4; i++)
                for (int j = 0; j < 4; j++)
                    if (Structure[i, j])
                        TetrisState.Grid[i + X, j + Y] = Color;

            TranslationTransition.FromTetromino(TetrisState, this, X - 1, Y, TransitionTime);
        }

        public bool Rotate(bool left)
        {
            int oldX = X, oldY = Y;
            //erase old location
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                    if (Structure[x, y])
                        TetrisState.Grid[x + X, y + Y] = Color.Transparent;

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
                    if (candidateStructure[i, j] && TetrisState.Grid[i + X, j + Y] != Color.Transparent)
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
                if (oldX != X || oldY != Y)
                    TranslationTransition.FromTetromino(TetrisState, this, oldX, oldY, TransitionTime);
                RotationTransition.FromTetromino(TetrisState, this, left ? -1 : -3, TransitionTime);

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
}
