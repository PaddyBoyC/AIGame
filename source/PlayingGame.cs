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

namespace AIGame.source
{
    internal class PlayingGame
    {
        private Texture2D blackSquare;
        private float fadeAmount = 0;
        private bool gameOver = false;
        private bool gameOverWin = false;

        private Rectangle endZone;

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

        public PlayingGame(ContentManager Content, GraphicsDevice GraphicsDevice)
        {
            blackSquare = Content.Load<Texture2D>("blacksquare");

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

        public void Update(GameTime gameTime)
        {

        }

        public void Draw(GameTime gameTime)
        {

        }
    }
}
