using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
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
        public Dictionary<Tuple<int, int>, LinkedList<Transition>> Transistions = new Dictionary<Tuple<int, int>, LinkedList<Transition>>();

        public bool GameOver = false;

        public bool Update()
        {
            if (!GameOver)
            {
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
                        {
                            for (int j = 0; j < Width; j++)
                            {
                                if (Grid[j, i] != Color.Transparent)
                                {
                                    GameOver = true;
                                    break;
                                }
                            }
                            if (GameOver)
                                break;
                        }

                        ClearFullLines();

                        current = new Tetromino(this);
                        List<Tetromino.Shapes> shapeNames = Tetromino.Structures.Keys.ToList();
                        Tetromino.Shapes newShape = shapeNames[r.Next(shapeNames.Count)];
                        current.Spawn(newShape, 3, 0, r.Next(4));
                    }
                }
            }
            return GameOver;
        }

        public void ReIndexTransitions(int x, int y)
        {

        }

        public void Draw(BasicEffect basicEffect, GameTime gametime)
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

            float gameTimeSeconds = (float)gametime.TotalGameTime.TotalSeconds;

            for (int x = 0; x < Width; x++)
            {
                for (int y = 0; y < Height; y++)
                {
                    if (Grid[x, y] != Color.Transparent)
                    {
                        VertexPositionColor[] square = new VertexPositionColor[8];

                        Color fill = Grid[x, y];
                        Color outline = OutlineColours[fill];

                        const float borderWidth = 0.05f;

                        float drawX = x;
                        float drawY = y;
                        float rotationAngle = 0;
                        Vector2 rotationCenter = Vector2.Zero;

                        Tuple<int, int> XY = Tuple.Create(x, y);

                        if (Transistions.ContainsKey(XY))
                        {
                            float scale = 1.0f / Transistions[XY].Count;
                            Transition tr = Transistions[XY].First();
                            if (tr.TransitionStartTime == 0)
                                tr.TransitionStartTime = gameTimeSeconds;

                            if ((gameTimeSeconds - tr.TransitionStartTime) / tr.TransitionTime > 1)
                                Transistions[XY].Remove(tr);
                            else
                            {
                                if (tr is RotationTransition)
                                {
                                    RotationTransition tRotate = tr as RotationTransition;
                                    rotationCenter = tRotate.Center;
                                    rotationAngle = tRotate.StartAngle * (float)Math.PI * (0.5f) * (gameTimeSeconds - tr.TransitionStartTime) / tr.TransitionTime;
                                }
                                else if (tr is TranslationTransition)
                                {
                                    TranslationTransition tTranslate = tr as TranslationTransition;
                                    drawX = tTranslate.StartX + (x - tTranslate.StartX) * (gameTimeSeconds - tr.TransitionStartTime) / tr.TransitionTime;
                                    drawY = tTranslate.StartY + (y - tTranslate.StartY) * (gameTimeSeconds - tr.TransitionStartTime) / tr.TransitionTime;
                                }
                            }
                            if (Transistions[XY].Count == 0)
                                Transistions.Remove(XY);
                        }

                        basicEffect.World = Matrix.CreateTranslation(new Vector3(-rotationCenter, 0)) * Matrix.CreateRotationZ(rotationAngle) * Matrix.CreateTranslation(new Vector3(rotationCenter, 0));

                        square[0] = new VertexPositionColor(new Vector3(drawX, drawY, 0), outline);
                        square[1] = new VertexPositionColor(new Vector3(drawX + 1, drawY, 0), outline);
                        square[2] = new VertexPositionColor(new Vector3(drawX + 1, drawY + 1, 0), outline);
                        square[3] = new VertexPositionColor(new Vector3(drawX, drawY + 1, 0), outline);

                        square[4] = new VertexPositionColor(new Vector3(drawX + borderWidth, drawY + borderWidth, 0), fill);
                        square[5] = new VertexPositionColor(new Vector3(drawX + 1 - borderWidth, drawY + borderWidth, 0), fill);
                        square[6] = new VertexPositionColor(new Vector3(drawX + 1 - borderWidth, drawY + 1 - borderWidth, 0), fill);
                        square[7] = new VertexPositionColor(new Vector3(drawX + borderWidth, drawY + 1 - borderWidth, 0), fill);

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
                    if (Grid[x, y] == Color.Transparent)
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
                    Grid[i, lookingAtY] = Color.Transparent;
                lookingAtY--;
            }
            return new int[1];
        }
    }
}
