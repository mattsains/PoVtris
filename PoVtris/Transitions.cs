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
        Tuple<int, int> TargetBlockPosition;
        public Tetris TetrisState;
        public Transition(Tetris tetrisState, Tuple<int, int> targetBlockPosition, float time, bool noAdd = false)
        {
            this.TetrisState = tetrisState;
            this.TargetBlockPosition = targetBlockPosition;
            this.TransitionTime = time;
            if (!noAdd)
            {
                Register();
            }
        }

        public void Register()
        {
            if (!TetrisState.Transistions.ContainsKey(TargetBlockPosition))
                TetrisState.Transistions.Add(TargetBlockPosition, new HashSet<Transition>());
            TetrisState.Transistions[TargetBlockPosition].Add(this);
        }

        public void Done()
        {
            TetrisState.Transistions[TargetBlockPosition].Remove(this);
            if (TetrisState.Transistions[TargetBlockPosition].Count == 0)
                TetrisState.Transistions.Remove(TargetBlockPosition);
        }
    }

    /// <summary>
    /// Represents a rotation from some rotation angle, to straight
    /// </summary>
    class RotationTransition : Transition
    {
        public int StartAngle, CurrentAngle;
        public Vector2 Center;
        public RotationTransition(Tetris tetrisState, Tuple<int, int> targetBlockPosition, Vector2 center, int startAngle, float time, bool noAdd = false)
            : base(tetrisState, targetBlockPosition, time, noAdd)
        {
            this.StartAngle = this.CurrentAngle = startAngle;
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
        public int StartX, StartY, CurrentX, CurrentY;
        public TranslationTransition(Tetris tetrisState, Tuple<int, int> targetBlockPosition, int startX, int startY, float time, bool noAdd = false)
            : base(tetrisState, targetBlockPosition, time, noAdd)
        {
            this.StartX = this.CurrentX = startX;
            this.StartY = this.CurrentY = startY;
        }

        public static void FromRow(Tetris tetrisState, int startRow, int targetRow, float time, bool noAdd = false)
        {
            for (int i = 0; i < Tetris.Width; i++)
            {
                if (tetrisState.Grid[i, targetRow])
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
