using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
#if diagnose
using System.Diagnostics;
#endif

namespace Tetris
{
    class Transition
    {
        public float TransitionTime;
        public float RotationAngle;
        public Vector2 RotationCenter;
        public Vector2 Translation;

        public float TransitionStartTime;
        public Tuple<int, int> FinalBlockTargetPosition;
        public Tetris TetrisState;

        public Transition(Tetris tetrisState, Tuple<int, int> finalBlockTargetPosition, float time, bool noAdd = false)
        {
            this.TetrisState = tetrisState;
            this.FinalBlockTargetPosition = finalBlockTargetPosition;
            this.TransitionTime = time;
            if (!noAdd)
            {
                Register();
            }
        }

        public void AddRotation(float rotationAngle, Vector2 rotationCenter)
        {
            Vector2 A = this.RotationCenter;
            Vector2 B = rotationCenter;

            Vector2 Bp = Vector2.Transform(B - A, Matrix.CreateRotationZ(-0.5f * this.RotationAngle)); //B rotated around A by -a/2
            Vector2 Ap = Vector2.Transform(A - B, Matrix.CreateRotationZ(0.5f * rotationAngle)); //A rotated around B by b/2


            //AB rotated around A
            float mA = (Bp.Y - A.Y) / (Bp.X - A.X);
            float cA = A.Y - mA * A.X;

            //AB rotated around B
            float mB = (B.Y - Ap.Y) / (B.X - Ap.X);
            float cB = B.Y - mB * B.X;

            //point of intersection of these lines:
            float intX = (cB - cA) / (mB - mA);
            float intY = mA * intX + cA;
            Vector2 intersect = new Vector2(intX, intY);

            float newRotationAngle = this.RotationAngle + rotationAngle;

            this.RotationAngle = newRotationAngle;
            this.RotationCenter = intersect;
        }

        public void AddTranslation(Vector2 translation)
        {
            this.Translation += translation;
        }
        public Matrix TransformationMatrix
        {
            get { return Matrix.CreateTranslation(new Vector3(this.RotationCenter, 0)) * Matrix.CreateRotationZ(this.RotationAngle) * Matrix.CreateTranslation(new Vector3(-this.RotationCenter + this.Translation, 0)); }
        }

        public void Register()
        {
            TetrisState.Transitions.Add(FinalBlockTargetPosition, this);
        }

        public void Done()
        {
            TetrisState.Transitions.Remove(FinalBlockTargetPosition);
        }
    }

    /// <summary>
    /// Represents a rotation from some rotation angle, to straight
    /// </summary>
    static class RotationTransition
    {
        public static void FromTetromino(Tetris tetrisState, Tetromino tetromino, int startAngle, float time)
        {
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                {
                    if (tetromino.Structure[x, y])
                    {
                        Tuple<int, int> XY = Tuple.Create(tetromino.X + x, tetromino.Y + y);
                        Transition t;
                        if (tetrisState.Transitions.ContainsKey(XY))
                        {
#if diagnose
                            Debug.WriteLine("({0},{1}) reused for rotation", XY.Item1, XY.Item2);
#endif
                            t = tetrisState.Transitions[XY];
                        }
                        else
                        {
#if diagnose
                            Debug.WriteLine("({0},{1}) Created for rotation", XY.Item1, XY.Item2);
#endif
                            t = new Transition(tetrisState, XY, time);
                        }

                        t.AddRotation(startAngle, new Vector2(tetromino.X + 2, tetromino.Y + 2));
                    }
                }
        }
    }

    static class TranslationTransition
    {
        public static void FromRow(Tetris tetrisState, int startRow, int targetRow, float time, bool noAdd = false)
        {
            for (int i = 0; i < Tetris.Width; i++)
            {
                if (tetrisState.Grid[i, targetRow] != Color.Transparent)
                {
                    Tuple<int, int> XY = Tuple.Create(i, targetRow);
                    Transition t;
                    if (tetrisState.Transitions.ContainsKey(XY))
                    {
#if diagnose
                        Debug.WriteLine("({0},{1}) reused for row translation", XY.Item1, XY.Item2);
#endif
                        t = tetrisState.Transitions[XY];
                    }
                    else
                    {
#if diagnose
                        Debug.WriteLine("({0},{1}) Created for row translation", XY.Item1, XY.Item2);
#endif
                        t = new Transition(tetrisState, XY, time);
                    }
                    t.AddTranslation(new Vector2(0, (targetRow - startRow)));
                }
            }
        }

        public static void FromTetromino(Tetris tetrisState, Tetromino tetromino, int startX, int startY, float time, bool noAdd = false)
        {
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                {
                    if (tetromino.Structure[x, y])
                    {
                        Tuple<int, int> XY = Tuple.Create(tetromino.X + x, tetromino.Y + y);
                        Transition t;
                        if (tetrisState.Transitions.ContainsKey(XY))
                            t = tetrisState.Transitions[XY];
                        else
                        {
#if diagnose
                            Debug.WriteLine("({0},{1}) Created for tet translation", XY.Item1, XY.Item2);
#endif
                            t = new Transition(tetrisState, XY, time);
                        }

                        t.AddTranslation(new Vector2((tetromino.X - startX), (tetromino.Y - startY)));
                    }
                }
        }
    }
}
