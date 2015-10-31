using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Input.Touch;
using Windows.Devices.Sensors;
using System;

namespace Tetris
{
    /// <summary>
    /// This is the main type for your game.
    /// </summary>
    public class Game1 : Game
    {
        GraphicsDeviceManager graphics;

        BasicEffect basicEffect;
        DynamicVertexBuffer vertexBuffer;
        DynamicIndexBuffer indexBuffer;

        Tetris TetrisState = new Tetris();


        public Game1()
        {
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
        }

        /// <summary>
        /// Allows the game to perform any initialization it needs to before starting to run.
        /// This is where it can query for any required services and load any non-graphic
        /// related content.  Calling base.Initialize will enumerate through any components
        /// and initialize them as well.
        /// </summary>
        protected override void Initialize()
        {
            OrientationSensor = OrientationSensor.GetDefault();
            if (OrientationSensor == null)
            {
                SimpleOrientationSensor = SimpleOrientationSensor.GetDefault();
                if (SimpleOrientationSensor == null)
                    throw new Exception("No way of determining orientation");
            }

            TouchPanel.EnabledGestures = GestureType.Hold | GestureType.Flick | GestureType.HorizontalDrag | GestureType.VerticalDrag | GestureType.DragComplete;

            vertexBuffer = new DynamicVertexBuffer(graphics.GraphicsDevice, typeof(VertexPositionColor), 8, BufferUsage.WriteOnly);
            indexBuffer = new DynamicIndexBuffer(graphics.GraphicsDevice, typeof(ushort), 36, BufferUsage.WriteOnly);

            basicEffect = new BasicEffect(graphics.GraphicsDevice); //(device, null);
            basicEffect.LightingEnabled = false;
            basicEffect.VertexColorEnabled = true;
            basicEffect.TextureEnabled = false;

            DepthStencilState depthBufferState = new DepthStencilState();
            depthBufferState.DepthBufferEnable = true;
            GraphicsDevice.DepthStencilState = depthBufferState;

            TetrisState.Initialize(graphics.GraphicsDevice);

            Windows.UI.ViewManagement.ApplicationView.GetForCurrentView().SuppressSystemOverlays = true;
            graphics.SupportedOrientations = DisplayOrientation.Portrait;

            base.Initialize();
        }



        OrientationSensor OrientationSensor;
        SimpleOrientationSensor SimpleOrientationSensor;
        Vector3 LastSimpleOrientation = Vector3.UnitY;

        public Vector3 GetOrientation()
        {
            if (OrientationSensor != null)
            {
                OrientationSensorReading orientation = OrientationSensor.GetCurrentReading();

                Matrix m = new Matrix();
                m.M11 = orientation.RotationMatrix.M11;
                m.M12 = orientation.RotationMatrix.M12;
                m.M13 = orientation.RotationMatrix.M13;
                m.M21 = orientation.RotationMatrix.M21;
                m.M22 = orientation.RotationMatrix.M22;
                m.M23 = orientation.RotationMatrix.M23;
                m.M31 = orientation.RotationMatrix.M31;
                m.M32 = orientation.RotationMatrix.M32;
                m.M33 = orientation.RotationMatrix.M33;


                return Vector3.Transform(Vector3.UnitY, m);
            }
            else if (SimpleOrientationSensor != null)
            {
                SimpleOrientation orientation = SimpleOrientationSensor.GetCurrentOrientation();

                switch (orientation)
                {
                    case SimpleOrientation.Facedown:
                    case SimpleOrientation.Faceup:
                        return LastSimpleOrientation;
                    case SimpleOrientation.NotRotated:
                        LastSimpleOrientation = Vector3.UnitY;
                        return LastSimpleOrientation;
                    case SimpleOrientation.Rotated180DegreesCounterclockwise:
                        LastSimpleOrientation = -Vector3.UnitY;
                        return LastSimpleOrientation;
                    case SimpleOrientation.Rotated270DegreesCounterclockwise:
                        LastSimpleOrientation = -Vector3.UnitX;
                        return LastSimpleOrientation;
                    case SimpleOrientation.Rotated90DegreesCounterclockwise:
                        LastSimpleOrientation = Vector3.UnitX;
                        return LastSimpleOrientation;
                    default:
                        throw new Exception("Unrecognised Orientation");
                }
            }
            else throw new Exception("Couldn't determine orientation");
        }

        /// <summary>
        /// LoadContent will be called once per game and is the place to load
        /// all of your content.
        /// </summary>
        protected override void LoadContent()
        {
        }

        /// <summary>
        /// UnloadContent will be called once per game and is the place to unload
        /// game-specific content.
        /// </summary>
        protected override void UnloadContent()
        {
            // TODO: Unload any non ContentManager content here
        }

        double lastSeconds = 0;

        float lastDragStart = 0;
        bool dragging = true;
        int currentOrientation = 0;
        /// <summary>
        /// Allows the game to run logic such as updating the world,
        /// checking for collisions, gathering input, and playing audio.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Update(GameTime gameTime)
        {

            Vector3 orientation = GetOrientation();

            float up = Vector3.Dot(orientation, Vector3.UnitY);
            float right = Vector3.Dot(orientation, Vector3.UnitX);

            int newOrientation;
            if (Math.Abs(up) > Math.Abs(right))
            {
                if (up > 0) newOrientation = 0;
                else newOrientation = 2;
            }
            else
            {
                if (right > 0) newOrientation = 3;
                else newOrientation = 1;
            }

            for (int difference = newOrientation - currentOrientation; difference != 0; )
            {
                bool left = difference > 0;

                if (Math.Abs(difference) > 2)
                    left = !left;

                bool success = TetrisState.current.Rotate(left);
                if (success)
                {
                    if (left)
                        currentOrientation = (currentOrientation + 1) % 4;
                    else
                        currentOrientation = currentOrientation == 0 ? 3 : ((currentOrientation - 1) % 4);

                    difference = newOrientation - currentOrientation;
                }
                else { break; }
            }

            if (TouchPanel.IsGestureAvailable)
            {
                GestureSample gesture = TouchPanel.ReadGesture();

                if (gesture.GestureType == GestureType.DragComplete)
                {
                    dragging = false;
                }
                else if (gesture.GestureType.HasFlag(GestureType.HorizontalDrag) || gesture.GestureType.HasFlag(GestureType.VerticalDrag))
                {
                    if (dragging == false)
                    {
                        dragging = true;
                        lastDragStart = gesture.Position.X;
                    }

                    if ((gesture.Position.X - lastDragStart) > 100)
                    {
                        TetrisState.current.MoveRight();
                        lastDragStart += 100;
                    }
                    if ((gesture.Position.X - lastDragStart) < -100)
                    {
                        TetrisState.current.MoveLeft();
                        lastDragStart -= 100;
                    }
                }
                else if (gesture.GestureType.HasFlag(GestureType.Flick))
                {
                    dragging = false;
                    if (gesture.Delta.X > 0)
                        TetrisState.current.MoveRight();
                    if (gesture.Delta.X < 0)
                        TetrisState.current.MoveLeft();
                }
            }


            if (gameTime.TotalGameTime.TotalSeconds - lastSeconds > 0.5f)
            {
                lastSeconds = gameTime.TotalGameTime.TotalSeconds;
                TetrisState.Update();
            }

            base.Update(gameTime);
        }
        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        /// <param name="gameTime">Provides a snapshot of timing values.</param>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.DarkGray);

            Matrix Projection = Matrix.CreateOrthographicOffCenter(0, 10, 22, 2, -1, 1);
            basicEffect.Projection = Projection;

            TetrisState.Draw(basicEffect, gameTime);

            base.Draw(gameTime);
        }
    }
}
