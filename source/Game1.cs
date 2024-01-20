using Apos.Gui;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using TiledSharp;
using System.Security.AccessControl;

namespace AIGame.source
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D renderTarget;

        public static float screenWidth;
        public static float screenHeight;

        #region UI
        IMGUI _ui;
        #endregion

        #region Managers
        private GameManager _gameManager;
        //private bool isGameOver = false;
        #endregion

        #region Tilemaps
        private TmxMap map;
        private TilemapManager tilemapManager;
        private Texture2D tileset;
        private List<Rectangle> collisionRects;
        private List<FakeFloor> fakeFloors;
        private Vector2 startPos;
        private Rectangle endRect;
        #endregion

        #region Player
        private Player player;
        private List<Bullet> bullets;
        private Texture2D bulletTexture;
        private int time_between_bullets;
        private int points = 0;
        private int player_health = 10;
        private int time_between_hurt = 20;
        private int hit_counter = 0;
        #endregion

        #region Enemy
        private List<Enemy> enemies;
        private List<Rectangle> enemyPathway;
        private List<Rectangle> birdSpawn;
        #endregion

        #region Bird
        private List<Bird> birds;
        #endregion

        //maybe implement comora instead
        #region Camera
        private camera camera;
        private Matrix transformMatrix;
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

            #region Tilemap
            map = new TmxMap("Content\\mainlevel2.tmx");
            tileset = Content.Load<Texture2D>("mainTileset\\" + map.Tilesets[0].Name.ToString());
            int tileWidth = map.Tilesets[0].TileWidth;
            int tileHeight = map.Tilesets[0].TileHeight;
            int tilesetTileWidth = tileset.Width / tileWidth;

            tilemapManager = new TilemapManager(map, tileset, tilesetTileWidth, tileWidth, tileHeight, GraphicsDevice, _spriteBatch);
            #endregion

            #region Collision
            collisionRects = new List<Rectangle>();
            fakeFloors = new List<FakeFloor>();

            foreach(var o in map.ObjectGroups["Collisions"].Objects)
            {
                if (o.Name == "")
                {
                    collisionRects.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
                }
            }

            foreach (var o in map.ObjectGroups["FakeFloors"].Objects)
            {
                fakeFloors.Add(new FakeFloor(Content.Load<Texture2D>("mainTileset\\fakefloor"), new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height), player));
            }

            #endregion

            _gameManager = new GameManager(endRect);

            #region Player
            foreach (var o in map.ObjectGroups["Points"].Objects)
            {
                if (o.Name == "PlayerStart")
                {
                    startPos = new Vector2((int)o.X, (int)o.Y);
                }
            }
            player = new Player(
                startPos,
                Content.Load<Texture2D>("mainCharacter\\maincharacter_idle"),
                Content.Load<Texture2D>("mainCharacter\\maincharacter_run"),
                Content.Load<Texture2D>("mainCharacter\\maincharacter_jump"),
                Content.Load<Texture2D>("mainCharacter\\maincharacter_fall"),
                hitbox => CheckLevelCollision(hitbox));

            #region Bullet
            bullets = new List<Bullet>();
            bulletTexture = Content.Load<Texture2D>("1 - Agent_Mike_Bullet (16 x 16)");
            #endregion

            #endregion

            #region Camera
            camera = new camera();
            #endregion

            #region Enemy
            enemyPathway = new List<Rectangle>();
            foreach (var o in map.ObjectGroups["EnemyPathways"].Objects)
            {
                enemyPathway.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }
            Texture2D enemyTexture = Content.Load<Texture2D>("statemachineEnemy\\2 - Martian_Red_Running (32 x 32)");
            enemies = new List<Enemy>();
            Enemy martian = new Enemy(
               enemyTexture,
               enemyPathway[0],
               player
                );

            enemies.Add(martian);
            martian = new Enemy(
               enemyTexture,
               enemyPathway[1],
               player
                );
            enemies.Add(martian);
            #endregion

            #region Bird
            List<(Texture2D, Texture2D)> birdTextures = new List<(Texture2D, Texture2D)>();
            birdTextures.Add((Content.Load<Texture2D>("flockingEnemy\\parrot1_idle"), Content.Load<Texture2D>("flockingEnemy\\parrot1_flying")));
            birdTextures.Add((Content.Load<Texture2D>("flockingEnemy\\parrot2_idle"), Content.Load<Texture2D>("flockingEnemy\\parrot2_flying")));
            birdTextures.Add((Content.Load<Texture2D>("flockingEnemy\\parrot3_idle"), Content.Load<Texture2D>("flockingEnemy\\parrot3_flying")));

            birds = new List<Bird>();
            Random rnd = new Random();

            foreach (var o in map.ObjectGroups["BirdSpawn"].Objects)
            {
                birds.Add(new Bird(new Vector2((float)o.X, (float)o.Y), birdTextures[rnd.Next(3)], birds, player));
            }
            #endregion

            renderTarget = new RenderTarget2D(GraphicsDevice, 960, 850);
        }

        protected override void Update(GameTime gameTime)
        {
            if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                Exit();

            #region UI
            GuiHelper.UpdateSetup(gameTime);

            _ui.UpdateAll(gameTime);

            Panel.Push().XY = new Vector2(0, 0);

            Label.Put($"Points: {points}");
            Label.Put($"Health: {player_health}");
            Label.Put($"pos {player.position}");
            Panel.Pop();

            Panel.Push().XY = new Vector2(screenWidth/2-100, screenHeight/2);
            #region Managers
            if (_gameManager.HasGameEnded(player.hitbox))
            {
                Label.Put("Game Ended!");
            }
            if (player_health <= 0)
            {
                Label.Put("Game Over!");
            }
            #endregion
            Panel.Pop();

            GuiHelper.UpdateCleanup();
            #endregion

            #region Enemy
            foreach (var enemy in enemies)
            {
                enemy.Update(gameTime);
                //isGameOver = enemy.hasHit(player.hitbox);

                if (enemy.hasHit(player.hitbox))
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

            #region Camera update
            Rectangle target = new Rectangle((int)player.position.X, (int)player.position.Y, 32, 32);
            transformMatrix = camera.Follow(target);

            #endregion

            #region Player

            #region Bullet

            if (player.isShooting)
            {
                if (time_between_bullets > 5) //&& bullets.ToArray().Length < 20) //number changes the amount of bullets you have in the level
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
                    time_between_bullets = 0;
                }
                else
                {
                    time_between_bullets++;
                }
            }

            foreach (var bullet in bullets.ToArray())
            {
                bullet.Update();

                foreach (var rect in collisionRects)
                {
                    if (rect.Intersects(bullet.hitbox))
                    {
                        bullets.Remove(bullet);
                        break;
                    }
                }
                foreach (var enemy in enemies.ToArray())
                {
                    if (bullet.hitbox.Intersects(enemy.hitbox))
                    {
                        bullets.Remove(bullet);
                        enemies.Remove(enemy);
                        points++;
                        break;
                    }
                }
            }

            #endregion


            #region Player Collision
            var initPos = player.position;
            player.Update(gameTime);
            #endregion

            #endregion

            #region FakeFloors

            foreach (var fakefloor in fakeFloors)
            {
                fakefloor.Update(gameTime);
            }

            #endregion

            base.Update(gameTime);
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

        public Rectangle? CheckLevelCollision(Rectangle hitbox)
        {
            foreach (var rectangle in collisionRects)
            {
                if (hitbox.Intersects(rectangle))
                {
                    return rectangle;
                }
            }
            foreach (var fakeFloor in fakeFloors)
            {
                if (fakeFloor.hasHit(hitbox))
                {
                    return fakeFloor.hitbox;
                }
            }
            return null;
        }
    }
}