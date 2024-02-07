using Apos.Gui;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using TiledSharp;
using AIGame.source.Flocking;

namespace AIGame.source
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D renderTarget;

        public static float screenWidth;
        public static float screenHeight;

        private PlayingGame game;

        #region UI
        IMGUI _ui;
        #endregion

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight = 930;
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.ApplyChanges();
            screenHeight = _graphics.PreferredBackBufferHeight;
            screenWidth = _graphics.PreferredBackBufferWidth;
            base.Initialize();
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            #region UI
            FontSystem fontSystem = FontSystemFactory.Create(GraphicsDevice, 2048, 2048);
            fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/PixeloidSansBold-PKnYd.ttf"));
            GuiHelper.Setup(this, fontSystem);
            _ui = new IMGUI();
            #endregion

            game = new PlayingGame();

            
            renderTarget = new RenderTarget2D(GraphicsDevice, 960, 850);
        }

        protected override void Update(GameTime gameTime)
        {
            #region UI
            GuiHelper.UpdateSetup(gameTime);

            _ui.UpdateAll(gameTime);

            Panel.Push().XY = new Vector2(0, 0);
            if (!gameOver && !gameOverWin)
            {
                Label.Put($"Spiders vanquished: {spidersKilled}");
                Label.Put($"Health: {player_health}");
                //Label.Put($"pos {player.position}"); //debug only
            }
            Panel.Pop();

            Panel.Push().XY = new Vector2(screenWidth/2 - 130, screenHeight/2 -200);
            if (player_health <= 0)
            {
                player.playerSpeed = 0;
                player_health = 0;
                Label.Put("You Fainted!");
                Panel.Pop();

                Panel.Push().XY = new Vector2(screenWidth / 2 - 320, screenHeight / 2 - 100);
                Label.Put("Press Enter to retreat to the surface...");
                Panel.Pop();

                Panel.Push().XY = new Vector2(screenWidth / 2 - 230, screenHeight / 2);
                Label.Put("Press Esc to exit the game");
                Panel.Pop();

                fadeAmount += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;
                if (fadeAmount > 1)
                {
                    fadeAmount = 1;
                }
                gameOver = true;
                if (Keyboard.GetState().IsKeyDown(Keys.Enter))
                {
                    Reset();
                }
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    Exit();
                }
            }

            Panel.Push().XY = new Vector2(screenWidth / 2 - 300, screenHeight / 2 - 200); //this isn't working as expected
            if (gameOverWin)
            {
                player.playerSpeed = 0;
                fadeAmount += (float)gameTime.ElapsedGameTime.TotalSeconds * 0.5f;


                Label.Put("To be continued...?");
                Panel.Pop();

                Panel.Push().XY = new Vector2(screenWidth / 2 - 800, screenHeight / 2 - 100);
                Label.Put($"Spiders Vanquished: {spidersKilled}");
                Panel.Pop(); 

                if (fadeAmount > 1)
                {
                    fadeAmount = 1;
                }
            }

            GuiHelper.UpdateCleanup();
            #endregion

            #region Enemy
            foreach (var enemy in enemies)
            {
                enemy.Update(gameTime);
                //isGameOver = enemy.hasHit(player.hitbox);

                if (!enemy.Dead && enemy.hasHit(player.hitbox))
                {
                    hit_counter++;
                    if (hit_counter > time_between_hurt)
                    {
                        player_health--;
                        hit_counter = 0;
                    }
                }
            }
            #endregion

            #region Bird
            foreach (var bird in birds)
            {
                bird.Update(gameTime);
            }
            #endregion

            #region SnowBat
            foreach (var snowBat in snowBats)
            {
                snowBat.Update(gameTime);
            }
            #endregion

            #region Bat
            foreach (var bat in bats)
            {
                bat.Update(gameTime);
            }
            #endregion

            #region Camera update
            Rectangle target = new Rectangle((int)player.position.X, (int)player.position.Y, 32, 32);
            transformMatrix = camera.Follow(target);

            #endregion

            #region BulletLogic

            mState = Mouse.GetState();

            if (mState.LeftButton == ButtonState.Pressed && mReleased == true)
            {
                if (time_between_bullets == 0) //&& bullets.ToArray().Length < 20) //number changes the amount of bullets you have in the level if pickups are added then maybe set an amount
                {
                    var temp_hitbox = new 
                       Rectangle((int)player.position.X+15, //both these numbers change where the bullet shoots from
                                 (int)player.position.Y+15,
                                 (int)bulletTexture.Width,
                                 (int)bulletTexture.Height);
                    if (player.effects == SpriteEffects.None)
                    {
                        bullets.Add(new Bullet(bulletTexture, 4, temp_hitbox));
                    }
                    if (player.effects == SpriteEffects.FlipHorizontally)
                    {
                        bullets.Add(new Bullet(bulletTexture, -4, temp_hitbox));
                    }
                }
                else
                {
                    time_between_bullets++;
                }
                mReleased = false;
            }
            if (mState.LeftButton == ButtonState.Released)
            {
                mReleased = true;
            }

            foreach (var bullet in bullets.ToArray())
            {
                bullet.Update();

                if(CheckLevelCollision(bullet.hitbox).HasValue)
                {
                    bullets.Remove(bullet);                   
                }

                foreach (var enemy in enemies)
                {
                    if (!enemy.Dead && bullet.hitbox.Intersects(enemy.hitbox))
                    {
                        bullets.Remove(bullet);
                        enemy.Dead = true;
                        spidersKilled++;
                        break;
                    }
                }
            }

            #endregion

            #region Player
            player.Update(gameTime);

            //check for picking up inventory object
            HashSet<InventoryObject> overlappingObjects = new();
            foreach (var obj in levelObjects)
            {
                if (player.hitbox.Intersects(obj.hitbox))
                {
                    overlappingObjects.Add(obj);
                    player.AddToInventory(obj);
                }
            }
            foreach (var obj in overlappingObjects)
            {
                levelObjects.Remove(obj);
            }
            if(player.hitbox.Intersects(endZone))
            {
                gameOverWin = true;
            }
            #endregion

            #region FakeFloors

            foreach (var fakefloor in fakeFloors)
            {
                fakefloor.Update(gameTime);
            }

            #endregion

            #region Door
            foreach (var door in doors)
            {
               door.Update(gameTime);
            }
            #endregion

            #region JungleFakeFloor
            foreach (var jungleFakeFloor in jungleFakeFloors)
            {
                jungleFakeFloor.Update(gameTime);
            }
            #endregion
        }

        public void DrawLevel(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(transformMatrix : transformMatrix);
            tilemapManager.Draw(_spriteBatch);

            foreach (var fakeFloor in fakeFloors)
            {
                fakeFloor.Draw(_spriteBatch, gameTime);
            }

            foreach (var door in doors)
            {
                door.Draw(_spriteBatch, gameTime);
            }

            foreach (var jungleFakeFloor in jungleFakeFloors)
            {
                jungleFakeFloor.Draw(_spriteBatch, gameTime);
            }

            #region Enemy
            foreach (var enemy in enemies)
            {
                enemy.Draw(_spriteBatch, gameTime);
            }
            #endregion

            #region Bird
            foreach (var bird in birds)
            {
                bird.Draw(_spriteBatch, gameTime);
            }
            #endregion

            #region Bat
            foreach (var bat in bats)
            {
                bat.Draw(_spriteBatch, gameTime);
            }
            #endregion

            #region SnowBat
            foreach (var snowBat in snowBats)
            {
                snowBat.Draw(_spriteBatch, gameTime);
            }
            #endregion

            #region InventoryObjects

            foreach (var obj in levelObjects)
            {
                obj.Draw(_spriteBatch, gameTime);
            }

            #endregion

            #region Player
            player.Draw(_spriteBatch, gameTime);

            #region Bullets
            foreach (var bullet in bullets.ToArray())
            {
                bullet.Draw(_spriteBatch);
            }
            #endregion

            #endregion           
            
            _spriteBatch.End();

            if (fadeAmount > 0)
            {
                _spriteBatch.Begin();
                _spriteBatch.Draw(blackSquare, new Rectangle(0, 0, (int)screenWidth, (int)screenHeight), Color.White * fadeAmount); //fades to black when dead
                _spriteBatch.End();
            }

            GraphicsDevice.SetRenderTarget(null);
        }

        protected override void Draw(GameTime gameTime)
        {
            DrawLevel(gameTime);         

            _spriteBatch.Begin(samplerState : SamplerState.PointClamp);

            _spriteBatch.Draw(renderTarget, new Vector2(0, 0), null, Color.White, 0f, new Vector2(), 2f, SpriteEffects.None, 0);

            _spriteBatch.End();
            _ui.Draw(gameTime);
            base.Draw(gameTime);
        }

        public struct LevelCollisionResult
        {
            public Rectangle rectangle;
            public bool slippery;
            public FakeFloor fakeFloor;
            public JungleFakeFloor jungleFakeFloor;
            public Door door;

            public LevelCollisionResult(Rectangle rectangle, bool slippery = false, FakeFloor fakeFloor = null, Door door = null, JungleFakeFloor jungleFakeFloor = null)
            {
                this.rectangle = rectangle;
                this.slippery = slippery;
                this.fakeFloor = fakeFloor;
                this.jungleFakeFloor = jungleFakeFloor;
                this.door = door;
            }
        }

        public LevelCollisionResult? CheckLevelCollision(Rectangle hitbox)
        {
            foreach (var rectangle in collisionRects)
            {
                if (hitbox.Intersects(rectangle))
                {
                    return new LevelCollisionResult(rectangle);
                }
            }
            foreach (var fakeFloor in fakeFloors)
            {
                if (fakeFloor.hasHit(hitbox))
                {
                    return new LevelCollisionResult(fakeFloor.hitbox, fakeFloor : fakeFloor);
                }
            }
            foreach (var slippyFloor in slipperyCollisionRects)
            {
                if (slippyFloor.Intersects(hitbox))
                {
                    return new LevelCollisionResult(slippyFloor, slippery : true);
                }
            }
            foreach (var door in doors)
            {
                if (door.hasHit(hitbox))
                {
                    return new LevelCollisionResult(door.hitbox, door : door);
                }
            }
            foreach (var jungleFakeFloor in jungleFakeFloors)
            {
                if (jungleFakeFloor.hasHit(hitbox))
                {
                    return new LevelCollisionResult(jungleFakeFloor.hitbox, jungleFakeFloor: jungleFakeFloor);
                }
            }
            return null;
        }

        private void Reset()
        {
            mReleased = true;
            player_health = 10;
            gameOver = false;
            fadeAmount = 0;
            player.playerSpeed = 140;
            player.position = startPos;
        }
    }   
}