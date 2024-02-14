using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using TiledSharp;
using AIGame.source.Flocking;
using Apos.Gui;
using System.Reflection.Metadata;
using Microsoft.Xna.Framework.Content;
using AIGame.source.States;

namespace AIGame.source
{
    public class PlayingGame : State
    {
        public bool Reset { get; private set; } = false;

        private Texture2D blackSquare;
        private float fadeAmount = 0;
        private bool gameOver = false;
        private bool gameOverWin = false;

        private Rectangle endZone;

        IMGUI _ui;
        SpriteBatch _spriteBatch;
        private RenderTarget2D renderTarget;

        GraphicsDevice GraphicsDevice;

        #region Managers
        private GameManager _gameManager;
        //private bool isGameOver = false;
        #endregion

        #region Tilemaps
        private TmxMap map;
        private TilemapManager tilemapManager;
        private Texture2D tileset;
        private List<Rectangle> collisionRects;
        private List<Rectangle> slipperyCollisionRects;
        private List<FakeFloor> fakeFloors;
        private List<JungleFakeFloor> jungleFakeFloors;
        private List<Door> doors;
        private Vector2 startPos;
        private Rectangle endRect;
        #endregion

        #region Player
        private Player player;
        private List<Bullet> bullets;
        private Texture2D bulletTexture;
        private int time_between_bullets;
        private int spidersKilled = 0;
        private int player_health = 10;
        private int time_between_hurt = 20;
        private int hit_counter = 0;
        #endregion

        #region Enemy
        private List<Enemy> enemies;
        private List<Rectangle> enemyPathway;
        #endregion

        #region BulletLogic
        MouseState mState;
        bool mReleased = true;
        #endregion

        #region Bird
        private List<Bird> birds;
        #endregion

        #region Bat
        private List<Bat> bats;
        #endregion

        #region SnowBat
        private List<SnowBat> snowBats;
        #endregion

        #region Camera
        private camera camera;
        private Matrix transformMatrix;
        #endregion

        #region InventoryObjects

        private HashSet<InventoryObject> levelObjects;

        #endregion

        public PlayingGame(ContentManager Content, GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch, RenderTarget2D renderTarget) :
            base(null)
        {
            _spriteBatch = spriteBatch;
            this.renderTarget = renderTarget;
            this.GraphicsDevice = GraphicsDevice;

            blackSquare = Content.Load<Texture2D>("blacksquare");
            _ui = new IMGUI();

            #region Tilemap
            map = new TmxMap("Content\\mainlevel2.tmx");
            tileset = Content.Load<Texture2D>("mainTileset\\" + map.Tilesets[0].Name.ToString());
            int tileWidth = map.Tilesets[0].TileWidth;
            int tileHeight = map.Tilesets[0].TileHeight;
            int tilesetTileWidth = tileset.Width / tileWidth;

            tilemapManager = new TilemapManager(map, tileset, tilesetTileWidth, tileWidth, tileHeight, GraphicsDevice, _spriteBatch);
            #endregion

            #region Player

            levelObjects = new HashSet<InventoryObject>();

            foreach (var o in map.ObjectGroups["Points"].Objects)
            {
                var pos = new Vector2((int)o.X, (int)o.Y);

                if (o.Name == "PlayerStart")
                {
                    startPos = pos;
                }
                else if (o.Name == "machete")
                {
                    levelObjects.Add(new Machete(pos, Content.Load<Texture2D>("mainCharacter\\machete")));
                }
                else if (o.Name == "pickaxe")
                {
                    levelObjects.Add(new Pickaxe(pos, Content.Load<Texture2D>("mainCharacter\\pickaxe")));
                }
                else if (o.Name == "key")
                {
                    levelObjects.Add(new Key(pos, Content.Load<Texture2D>("mainCharacter\\key")));
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

            #region Collision
            collisionRects = new List<Rectangle>();
            slipperyCollisionRects = new List<Rectangle>();
            fakeFloors = new List<FakeFloor>();
            doors = new List<Door>();
            jungleFakeFloors = new List<JungleFakeFloor>();


            foreach (var o in map.ObjectGroups["Collisions"].Objects)
            {
                if (o.Name == "")
                {
                    collisionRects.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
                }
            }

            foreach (var o in map.ObjectGroups["SlippyFloors"].Objects)
            {
                if (o.Name == "")
                {
                    slipperyCollisionRects.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
                }
            }

            foreach (var o in map.ObjectGroups["JungleFakeFloors"].Objects)
            {
                Animation vinesAnim = new Animation(Content.Load<Texture2D>("maincharacter\\vinesbreaking"), width: 48, height: 48, millisecondsPerFrame: 50);
                vinesAnim.Playing = false;
                vinesAnim.Loop = false;
                jungleFakeFloors.Add(new JungleFakeFloor(vinesAnim, new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height), player));
            }

            foreach (var o in map.ObjectGroups["FakeFloors"].Objects)
            {
                fakeFloors.Add(new FakeFloor(Content.Load<Texture2D>("mainTileset\\fakefloor"), new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height), player));
            }

            foreach (var o in map.ObjectGroups["LockedDoor"].Objects)
            {
                Animation dooranim = new Animation(Content.Load<Texture2D>("maincharacter\\icedoorunlocking"), millisecondsPerFrame: 50);
                dooranim.Playing = false;
                dooranim.Loop = false;
                doors.Add(new Door(dooranim, new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height), player));
            }

            foreach (var o in map.ObjectGroups["zones"].Objects)
            {
                if (o.Name == "endZone")
                {
                    endZone = new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height);
                }
            }
            #endregion

            _gameManager = new GameManager(endRect);

            #region Camera
            camera = new camera();
            #endregion

            #region Enemy
            enemyPathway = new List<Rectangle>();
            foreach (var o in map.ObjectGroups["EnemyPathways"].Objects)
            {
                enemyPathway.Add(new Rectangle((int)o.X, (int)o.Y, (int)o.Width, (int)o.Height));
            }
            Texture2D jungleSpiderTexture = Content.Load<Texture2D>("statemachineEnemy\\junglespider_walking");
            Texture2D jungleSpiderAlertTexture = Content.Load<Texture2D>("statemachineEnemy\\junglespider_alert");
            Texture2D jungleSpiderDeadTexture = Content.Load<Texture2D>("statemachineEnemy\\junglespider_death");
            Texture2D jungleSpiderJumpTexture = Content.Load<Texture2D>("statemachineEnemy\\junglespider_startle");

            Texture2D snowSpiderTexture = Content.Load<Texture2D>("statemachineEnemy\\snowSpider_walking");
            Texture2D snowSpiderAlertTexture = Content.Load<Texture2D>("statemachineEnemy\\snowSpider_alert");
            Texture2D snowSpiderDeadTexture = Content.Load<Texture2D>("statemachineEnemy\\snowspider_death");
            Texture2D snowSpiderJumpTexture = Content.Load<Texture2D>("statemachineEnemy\\snowspider_startle");

            Texture2D spiderTexture = Content.Load<Texture2D>("statemachineEnemy\\spider_walking");
            Texture2D spiderAlertTexture = Content.Load<Texture2D>("statemachineEnemy\\spider_alert");
            Texture2D spiderDeadTexture = Content.Load<Texture2D>("statemachineEnemy\\spider_death");
            Texture2D spiderJumpTexture = Content.Load<Texture2D>("statemachineEnemy\\spider_startle");

            enemies = new List<Enemy>();
            Enemy jungleSpider = new Enemy(
               jungleSpiderTexture, jungleSpiderAlertTexture, jungleSpiderDeadTexture, jungleSpiderJumpTexture,
               enemyPathway[0],
               player
                );
            enemies.Add(jungleSpider);

            jungleSpider = new Enemy(
               jungleSpiderTexture, jungleSpiderAlertTexture, jungleSpiderDeadTexture, jungleSpiderJumpTexture,
               enemyPathway[1],
               player
                );
            enemies.Add(jungleSpider);

            jungleSpider = new Enemy(
               jungleSpiderTexture, jungleSpiderAlertTexture, jungleSpiderDeadTexture, jungleSpiderJumpTexture,
               enemyPathway[2],
               player
                );
            enemies.Add(jungleSpider);

            jungleSpider = new Enemy(
            jungleSpiderTexture, jungleSpiderAlertTexture, jungleSpiderDeadTexture, jungleSpiderJumpTexture,
            enemyPathway[5],
            player
            );
            enemies.Add(jungleSpider);

            jungleSpider = new Enemy(
            jungleSpiderTexture, jungleSpiderAlertTexture, jungleSpiderDeadTexture, jungleSpiderJumpTexture,
            enemyPathway[6],
            player
            );
            enemies.Add(jungleSpider);

            jungleSpider = new Enemy(
            jungleSpiderTexture, jungleSpiderAlertTexture, jungleSpiderDeadTexture, jungleSpiderJumpTexture,
            enemyPathway[7],
            player
            );
            enemies.Add(jungleSpider);

            Enemy spider = new Enemy(
               spiderTexture, spiderAlertTexture, spiderDeadTexture, spiderJumpTexture,
               enemyPathway[3],
               player
                );
            enemies.Add(spider);

            spider = new Enemy(
               spiderTexture, spiderAlertTexture, spiderDeadTexture, spiderJumpTexture,
               enemyPathway[4],
               player
                );
            enemies.Add(spider);

            spider = new Enemy(
               spiderTexture, spiderAlertTexture, spiderDeadTexture, spiderJumpTexture,
               enemyPathway[14],
               player
                );
            enemies.Add(spider);

            spider = new Enemy(
               spiderTexture, spiderAlertTexture, spiderDeadTexture, spiderJumpTexture,
               enemyPathway[17],
               player
                );
            enemies.Add(spider);

            spider = new Enemy(
               spiderTexture, spiderAlertTexture, spiderDeadTexture, spiderJumpTexture,
               enemyPathway[18],
               player
                );
            enemies.Add(spider);

            Enemy snowSpider = new Enemy(
                snowSpiderTexture, snowSpiderAlertTexture, snowSpiderDeadTexture, snowSpiderJumpTexture,
                enemyPathway[8],
                player
            );
            enemies.Add(snowSpider);

            snowSpider = new Enemy(
                snowSpiderTexture, snowSpiderAlertTexture, snowSpiderDeadTexture, snowSpiderJumpTexture,
                enemyPathway[9],
                player
            );
            enemies.Add(snowSpider);

            snowSpider = new Enemy(
                snowSpiderTexture, snowSpiderAlertTexture, snowSpiderDeadTexture, snowSpiderJumpTexture,
                enemyPathway[10],
                player
            );
            enemies.Add(snowSpider);

            snowSpider = new Enemy(
                snowSpiderTexture, snowSpiderAlertTexture, snowSpiderDeadTexture, snowSpiderJumpTexture,
                enemyPathway[11],
                player
            );
            enemies.Add(snowSpider);

            snowSpider = new Enemy(
                snowSpiderTexture, snowSpiderAlertTexture, snowSpiderDeadTexture, snowSpiderJumpTexture,
                enemyPathway[15],
                player
            );
            enemies.Add(snowSpider);

            spider = new Enemy(
                jungleSpiderTexture, jungleSpiderAlertTexture, jungleSpiderDeadTexture, jungleSpiderJumpTexture,
                enemyPathway[12],
                player
                );
            enemies.Add(spider);

            spider = new Enemy(
                jungleSpiderTexture, jungleSpiderAlertTexture, jungleSpiderDeadTexture, jungleSpiderJumpTexture,
                enemyPathway[13],
                player
                );
            enemies.Add(spider);

            snowSpider = new Enemy(
                snowSpiderTexture, snowSpiderAlertTexture, snowSpiderDeadTexture, snowSpiderJumpTexture,
                enemyPathway[16],
                player
            );
            enemies.Add(snowSpider);
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

            #region Bat
            List<(Texture2D, Texture2D)> batTextures = new List<(Texture2D, Texture2D)>();
            batTextures.Add((Content.Load<Texture2D>("flockingEnemy\\bat_idle"), Content.Load<Texture2D>("flockingEnemy\\bat_flying")));
            bats = new List<Bat>();

            foreach (var o in map.ObjectGroups["BatSpawn"].Objects)
            {
                bats.Add(new Bat(new Vector2((float)o.X, (float)o.Y), batTextures[rnd.Next(1)], bats, player));
            }

            #endregion

            #region SnowBat
            List<(Texture2D, Texture2D)> snowbatTextures = new List<(Texture2D, Texture2D)>();
            snowbatTextures.Add((Content.Load<Texture2D>("flockingEnemy\\snowbat_idle"), Content.Load<Texture2D>("flockingEnemy\\snowbat_flying")));
            snowBats = new List<SnowBat>();

            foreach (var o in map.ObjectGroups["SnowBatSpawn"].Objects)
            {
                snowBats.Add(new SnowBat(new Vector2((float)o.X, (float)o.Y), snowbatTextures[rnd.Next(1)], snowBats, player));
            }
            #endregion

        }

        public override void OnUpdate()
        {
            GameTime gameTime = Game1.LastGameTime;
            float screenWidth = Game1.screenWidth;
            float screenHeight = Game1.screenHeight;

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

            Panel.Push().XY = new Vector2(screenWidth / 2 - 130, screenHeight / 2 - 200);
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
                    Reset = true;
                }
                if (GamePad.GetState(PlayerIndex.One).Buttons.Back == ButtonState.Pressed || Keyboard.GetState().IsKeyDown(Keys.Escape))
                {
                    //Exit();
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
                       Rectangle((int)player.position.X + 15, //both these numbers change where the bullet shoots from
                                 (int)player.position.Y + 15,
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

                if (CheckLevelCollision(bullet.hitbox).HasValue)
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
            if (player.hitbox.Intersects(endZone))
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

        public void Draw(GameTime gameTime)
        {
            DrawLevel(gameTime);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);

            _spriteBatch.Draw(renderTarget, new Vector2(0, 0), null, Color.White, 0f, new Vector2(), 2f, SpriteEffects.None, 0);

            _spriteBatch.End();
            _ui.Draw(gameTime);
        }

        public void DrawLevel(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.CornflowerBlue);

            _spriteBatch.Begin(transformMatrix: transformMatrix);
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
                _spriteBatch.Draw(blackSquare, new Rectangle(0, 0, (int)Game1.screenWidth, (int)Game1.screenHeight), Color.White * fadeAmount); //fades to black when dead
                _spriteBatch.End();
            }

            GraphicsDevice.SetRenderTarget(null);
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
                    return new LevelCollisionResult(fakeFloor.hitbox, fakeFloor: fakeFloor);
                }
            }
            foreach (var slippyFloor in slipperyCollisionRects)
            {
                if (slippyFloor.Intersects(hitbox))
                {
                    return new LevelCollisionResult(slippyFloor, slippery: true);
                }
            }
            foreach (var door in doors)
            {
                if (door.hasHit(hitbox))
                {
                    return new LevelCollisionResult(door.hitbox, door: door);
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
    }
}
