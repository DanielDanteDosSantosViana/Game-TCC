#region File Description
//-----------------------------------------------------------------------------
// GeneratedGeometry.cs
//
// Microsoft XNA Community Game Platform
// Copyright (C) Microsoft Corporation. All rights reserved.
//-----------------------------------------------------------------------------
#endregion

#region Using Statements
using System;
using System.Diagnostics;
using System.Collections.Generic;
using System.Linq;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Audio;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.GamerServices;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Microsoft.Xna.Framework.Media;
#endregion

namespace GeneratedGeometry
{
    /// <summary>
    /// Sample showing how to use geometry that is programatically
    /// generated as part of the content pipeline build process.
    /// </summary>
    public class GeneratedGeometryGame : Microsoft.Xna.Framework.Game
    {
        #region Fields

        GraphicsDeviceManager graphics;
        SpriteBatch spriteBatch;
        SpriteFont font;
        Texture2D pix;

        Ship.Camera camera;
        float aspectRatio;

        Ship.CObject terrain;
        Sky sky;
        Ship.LirouShip ship;
        Ship.Ring[] rings = new Ship.Ring[10];
        List<Ship.CObject> collidableObjects;

        Ship.TerrainInfo terrainInfo;


        #endregion

        #region Initialization


        public GeneratedGeometryGame()
        {
            graphics = new GraphicsDeviceManager(this);
            //graphics.IsFullScreen = true;
            graphics.PreferredBackBufferWidth = 800;
            graphics.PreferredBackBufferHeight = 600;
            //IsMouseVisible = true;
            
            terrain = new Ship.CObject();
            terrain.ID = "terrain";

            aspectRatio = (float)graphics.PreferredBackBufferWidth / (float)graphics.PreferredBackBufferHeight;
            Content.RootDirectory = "Content";

            collidableObjects = new List<Ship.CObject>();

            ship = new Ship.LirouShip(new Vector3(0, 0 , 500), new Vector3(0, 0, 0), 1.0f, (float)(Math.PI / 75), 0.002f);
            collidableObjects.Add(ship);

            int xPos = -2000;
            for( int i = 0; i < rings.Length; i++)
            {
                rings[i] = new Ship.Ring(new Vector3(xPos, 0, 0), (float)(Math.PI / 50), 0.01f);
                collidableObjects.Add(rings[i]);
                xPos += 650;
            }
            camera = new Ship.Camera(35, ship.Position);
            

            
            //collidableObjects.Add(terrain);
            
            

            #if WINDOWS_PHONE
                        // Frame rate is 30 fps by default for Windows Phone.
                        TargetElapsedTime = TimeSpan.FromTicks(333333);

                        graphics.IsFullScreen = true;
            #endif
        }


        /// <summary>
        /// Load your graphics content.
        /// </summary>
        protected override void LoadContent()
        {
            spriteBatch = new SpriteBatch(GraphicsDevice);
            font = Content.Load<SpriteFont>(@"Font1");
            pix = new Texture2D(GraphicsDevice, 1, 1, false, SurfaceFormat.Color);
            pix.SetData(new[] { Color.White }); // so that we can draw whatever color we want on top of it

            terrain.Model = Content.Load<Model>("Terrain/terrain");
            terrainInfo = terrain.Model.Tag as Ship.TerrainInfo;
            if (terrainInfo == null)
            // Avisar caso não o objeto Tag não seja associado ao modelo
            {
                string message = "O modelo do Terreno não um tem TerrainInfo " +
                    "associado. Verifique se está usando " +
                    "TerrainProcessor";
                throw new InvalidOperationException(message);
            }
            sky = Content.Load<Sky>("sky");
            ship.LoadContent(this.Content);
            foreach (Ship.Ring ring in rings)
            {
                ring.LoadContent(this.Content);
            }
            
        }

        
        #endregion

        #region Update and Draw


        /// <summary>
        /// Allows the game to run logic.
        /// </summary>
        int points;
        protected override void Update(GameTime gameTime)
        {
            
            KeyboardState ks = Keyboard.GetState();

            HandleInput();
            collisionTerrain(ks);
            camera.Update(Mouse.GetState(), gameTime, ship.Position, ship.Rotation, graphics);
            isColliding();
            foreach (Ship.Ring ring in rings)
            {
                ring.Animate();
            }

            base.Update(gameTime);
        }

        #region Collision's Methods

        Vector3 position;
        float getHeight;
        bool isColl = false,tColl = false;

        public void collisionTerrain(KeyboardState ks)
        {
            position = ship.Position;
            float terrainScale = terrainInfo.TerrainScale();
            ship.Update(terrainScale, ks, camera, collidableObjects);
            if (terrainInfo.IsOnHeightmap(position))
            {
                getHeight = terrainInfo.GetHeight(position);
                if (position.Y/terrainScale <= getHeight)
                {
                    if(!(tColl))
                    {
                        ship.collReverse();
                        tColl = true;
                    }
                }
                else
                    tColl = false;
            }
            else
            {
                ship.restartPos();
                //Console.WriteLine("Yooo");
            }
            
            if (ks.IsKeyDown(Keys.RightShift))
            {
                Debug.WriteLine("" + position.Y/terrainScale + getHeight );
                Debug.Indent();
            }
        }
        
        public bool isColliding()
        {
            int auxColl = 0;
            Matrix worldship = Matrix.CreateTranslation(ship.Position);
            foreach (Ship.Ring ring in rings)
            {
                Matrix worldring = Matrix.CreateTranslation(ring.Position);
                Ship.CObject r = ring;
                if (ship.getCollision(ref r, worldship, worldring))
                {
                    if (!isColl)
                    {
                        points += ring.GetPoint();
                        isColl = true;
                        Console.WriteLine("     " + points);
                    }
                }
                else
                    auxColl++;
            }
            if (auxColl == rings.Length)
                isColl = false;

            return isColl;
        }
        
        #endregion

        /// <summary>
        /// This is called when the game should draw itself.
        /// </summary>
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice device = graphics.GraphicsDevice;
            
            device.Clear(Color.Black);
            
            Matrix projection = Matrix.CreatePerspectiveFieldOfView(MathHelper.PiOver4, device.Viewport.AspectRatio, 1, 10000);
            Matrix view = Matrix.CreateLookAt(camera.Position, ship.Position, Vector3.Up);
            graphics.GraphicsDevice.Clear(Color.CornflowerBlue);

            // Draw the terrain first, then the sky. This is faster than
            // drawing the sky first, because the depth buffer can skip
            // bothering to draw sky pixels that are covered up by the
            // terrain. This trick works because the code used to draw
            // the sky forces all the sky vertices to be as far away as
            // possible, and turns depth testing on but depth writes off.

            

            sky.Draw(view, projection);

            // If there was any alpha blended translucent geometry in
            // the scene, that would be drawn here, after the sky.
            spriteBatch.Begin();
            terrain.EditEffects();
            terrain.Draw(view, projection);

            ship.Draw(aspectRatio, view);
            foreach (Ship.Ring ring in rings)
            {
                ring.Draw(view, projection, aspectRatio);
            } 
            spriteBatch.DrawString(font, " " + points.ToString() + " pontos",
                            new Vector2(graphics.PreferredBackBufferWidth * .8f,graphics.PreferredBackBufferHeight *.1f )
                                , Color.ForestGreen);

            /*spriteBatch.Draw(pix, new Rectangle(
                graphics.PreferredBackBufferWidth -100,
                graphics.PreferredBackBufferHeight - 100,
                (int)(terrainInfo.HeightmapWidth/50),
                (int)(terrainInfo.HeightmapHeight)/50),
                Color.GreenYellow);*/
            int rS = 45;
            Rectangle titleSafeRectangle = new Rectangle(
                (int)(graphics.PreferredBackBufferWidth * 0.01f),
                (int)(graphics.PreferredBackBufferHeight * 0.01f),
                (int)(terrainInfo.HeightmapWidth / rS),
                (int)(terrainInfo.HeightmapHeight) / rS);
            DrawMap(titleSafeRectangle, 5, 3, Color.ForestGreen, rS);

            spriteBatch.End();

            base.Draw(gameTime);

        }

        private void DrawMap(Rectangle rectangleToDraw, int thicknessOfBorder, int thicknessOfPoint, Color borderColor, int rS)
        {
            // Draw top line
            spriteBatch.Draw(pix, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, rectangleToDraw.Width, thicknessOfBorder), borderColor);

            // Draw left line
            spriteBatch.Draw(pix, new Rectangle(rectangleToDraw.X, rectangleToDraw.Y, thicknessOfBorder, rectangleToDraw.Height), borderColor);

            // Draw right line
            spriteBatch.Draw(pix, new Rectangle((rectangleToDraw.X + rectangleToDraw.Width - thicknessOfBorder),
                                            rectangleToDraw.Y,
                                            thicknessOfBorder,
                                            rectangleToDraw.Height), borderColor);
            // Draw bottom line
            spriteBatch.Draw(pix, new Rectangle(rectangleToDraw.X,
                                            rectangleToDraw.Y + rectangleToDraw.Height - thicknessOfBorder,
                                            rectangleToDraw.Width,
                                            thicknessOfBorder), borderColor);
            // Draw the Ship
            Vector3 posOnMap = terrainInfo.WhereOnTheMap(ship.Position);
            spriteBatch.Draw(pix, new Rectangle((int)posOnMap.X/rS + rectangleToDraw.X - thicknessOfBorder,
                (int)posOnMap.Z/ rS + rectangleToDraw.Y - thicknessOfBorder,
                thicknessOfPoint,
                thicknessOfPoint), Color.Brown);

            foreach (Ship.Ring ring in rings)
            {
                Vector3 ringOnMap = terrainInfo.WhereOnTheMap(ring.Position);
                
                spriteBatch.Draw(pix, new Rectangle((int)ringOnMap.X / rS + rectangleToDraw.X - thicknessOfBorder,
                    (int)ringOnMap.Z / rS + rectangleToDraw.Y - thicknessOfBorder,
                    thicknessOfPoint,
                    thicknessOfPoint), Color.CornflowerBlue);
            } 
        }

        /// <summary>
        /// Helper for drawing the terrain model.
        /// </summary>



        #endregion

        #region Handle Input


        /// <summary>
        /// Handles input for quitting the game.
        /// </summary>
        private void HandleInput()
        {
            KeyboardState currentKeyboardState = Keyboard.GetState();
            GamePadState currentGamePadState = GamePad.GetState(PlayerIndex.One);

            // Check for exit.
            if (currentKeyboardState.IsKeyDown(Keys.Escape) ||
                currentGamePadState.Buttons.Back == ButtonState.Pressed)
            {
                Exit();
            }
        }


        #endregion
    }


    #region Entry Point

    /// <summary>
    /// The main entry point for the application.
    /// </summary>
    static class Program
    {
        static void Main()
        {
            using (GeneratedGeometryGame game = new GeneratedGeometryGame())
            {
                game.Run();
            }
        }
    }

    #endregion
}
