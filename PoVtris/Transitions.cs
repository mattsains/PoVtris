using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Tetris
{
    abstract class Transition
    {
        public float TransitionTime;
        public float TransitionStartTime;
        Tuple<int, int> FinalBlockTargetPosition;
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

        public void Register()
        {
            if (!TetrisState.Transistions.ContainsKey(FinalBlockTargetPosition))
                TetrisState.Transistions.Add(FinalBlockTargetPosition, new LinkedList<Transition>());
            TetrisState.Transistions[FinalBlockTargetPosition].AddLast(this);
        }

        public void Done()
        {
            TetrisState.Transistions[FinalBlockTargetPosition].Remove(this);
            if (TetrisState.Transistions[FinalBlockTargetPosition].Count == 0)
                TetrisState.Transistions.Remove(FinalBlockTargetPosition);
        }
    }

    /// <summary>
    /// Represents a rotation from some rotation angle, to straight
    /// </summary>
    class RotationTransition : Transition
    {
        public int StartAngle;
        public Vector2 Center;
        public RotationTransition(Tetris tetrisState, Tuple<int, int> finalBlockTargetPosition, Vector2 center, int startAngle, float time, bool noAdd = false)
            : base(tetrisState, finalBlockTargetPosition, time, noAdd)
        {
            this.StartAngle = startAngle;
            this.Center = center;
        }

        public static void FromTetromino(Tetris tetrisState, Tetromino tetromino, int startAngle, float time, bool noAdd = false)
        {
            for (int x = 0; x < 4; x++)
                for (int y = 0; y < 4; y++)
                {
                    if (tetromino.Structure[x, y])
                    {
                        new RotationTransition(tetrisState, Tuple.Create(tetromino.X + x, tetromino.Y + y), new Vector2(tetromino.X + 2, tetromino.Y + 2), startAngle, time, noAdd);
                    }
                }
        }
    }

    class TranslationTransition : Transition
    {
        public int StartX, StartY;
        public TranslationTransition(Tetris tetrisState, Tuple<int, int> finalBlockTargetPosition, int startX, int startY, float time, bool noAdd = false)
            : base(tetrisState, finalBlockTargetPosition, time, noAdd)
        {
            this.StartX = startX;
            this.StartY = startY;
        }

        public static void FromRow(Tetris tetrisState, int startRow, int targetRow, float time, bool noAdd = false)
        {
            for (int i = 0; i < Tetris.Width; i++)
            {
                if (tetrisState.Grid[i, targetRow] != Color.Transparent)
                {
                    new TranslationTransition(tetrisState, Tuple.Create(i, targetRow), i, startRow, time, noAdd);
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
                        new TranslationTransition(tetrisState, Tuple.Create(tetromino.X + x, tetromino.Y + y), startX + x, startY + y, time, noAdd);
                    }
                }
        }
    }
}
