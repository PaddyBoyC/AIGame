using AIGame.source;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using static AIGame.source.Entity;

namespace AIGame.source
{
    public class Player : Entity
    {
        public enum CurrentAnimation
        {
            Idle,
            Run,
            Jumping,
            Falling,
        }

        public Vector2 velocity;
        public SpriteEffects effects;

        public float playerSpeed = 140;
        public float fallAccel = 1500;
        public float maxFallSpeed = 950;
        public float jumpSpeed = -400;
        public bool isFalling = true;
        public bool isJumping;
        public bool isShooting;

        bool onIce = false;

        public float FreezeTimer { get; set; }

        private HashSet<InventoryObject> inventory;

        Animation[] playerAnimation;
        CurrentAnimation playerAnimationController;

        Func<Rectangle, Game1.LevelCollisionResult?> levelCollisionFunc;


        Vector2 hitboxOffset;

        public Player(Vector2 position, Texture2D idleSprite, Texture2D runSprite, Texture2D jumpSprite, Texture2D fallSprite, Func<Rectangle, Game1.LevelCollisionResult?> levelCollisionFunc)
        {
            playerAnimation = new Animation[4];

            this.position = position;
            this.levelCollisionFunc = levelCollisionFunc;
            inventory = new HashSet<InventoryObject>();

            velocity = new Vector2();
            effects = SpriteEffects.None;

            playerAnimation[0] = new Animation(idleSprite, millisecondsPerFrame: 100);
            playerAnimation[1] = new Animation(runSprite, millisecondsPerFrame: 100);
            playerAnimation[2] = new Animation(jumpSprite, millisecondsPerFrame: 100);
            playerAnimation[3] = new Animation(fallSprite, millisecondsPerFrame: 600);

            hitboxOffset = new Vector2(11, 11);
            hitbox = new Rectangle((int)position.X + (int)hitboxOffset.X, (int)position.Y + (int)hitboxOffset.Y, 12, 21);
        }

        public override void Update(GameTime gameTime)
        {
            float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;

            FreezeTimer -= dt;
            if (FreezeTimer > 0)
            {
                return;
            }

            KeyboardState keyboard = Keyboard.GetState();

            playerAnimationController = CurrentAnimation.Idle;

            isShooting = keyboard.IsKeyDown(Keys.Enter);

            Move(keyboard, dt);
        
            //apply x movement and check for collision
            position.X += velocity.X * dt;
            hitbox.X = (int)position.X + (int)hitboxOffset.X;
            var collisionResult = levelCollisionFunc(hitbox);
            if (collisionResult.HasValue)
            {
                var collidingRectangle = collisionResult.Value.rectangle;
                if (velocity.X > 0)
                {
                    hitbox.X = collidingRectangle.X - hitbox.Width - 1;
                }
                else
                {
                    hitbox.X = collidingRectangle.X + collidingRectangle.Width + 1;
                }
                velocity.X = 0;
                position.X = hitbox.X - hitboxOffset.X;
                if (collisionResult.Value.door != null && Has<Key>())
                {
                    collisionResult.Value.door.Unlock();
                }
            }

            if (isJumping)
            {
                playerAnimationController = CurrentAnimation.Jumping;
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.Space) && IsGrounded())
                {
                    isJumping = true;
                    isFalling = false;
                    velocity.Y = jumpSpeed;
                }
            }

            if (!IsGrounded())
            {
                velocity.Y += fallAccel * dt;
            }                     

            //apply y movement and check for collision
            position.Y += velocity.Y * dt;
            hitbox.Y = (int)position.Y + (int)hitboxOffset.Y;
            collisionResult = levelCollisionFunc(hitbox);
            if (collisionResult.HasValue)
            {
                var collidingRectangle = collisionResult.Value.rectangle;
                if (velocity.Y > 0)
                {
                    isFalling = false;
                    isJumping = false;
                    hitbox.Y = collidingRectangle.Y - hitbox.Height;
                    onIce = collisionResult.Value.slippery;
                    if (collisionResult.Value.fakeFloor != null && Has<Pickaxe>())
                    {
                        collisionResult.Value.fakeFloor.StoodOn();
                    }                  
                }
                else
                {
                    hitbox.Y = collidingRectangle.Y + collidingRectangle.Height + 1;
                }
                velocity.Y = 0;                   
                position.Y = hitbox.Y - hitboxOffset.Y;
            }

            if (velocity.Y != 0)
            {
                playerAnimationController = CurrentAnimation.Falling;
            }
        }

        private void Move(KeyboardState keyboard, float dt)
        {
            float targetVel = 0;
            float friction = IsGrounded() ? (onIce ? 1 : 8) : 4;

            if (keyboard.IsKeyDown(Keys.A))
            {
                targetVel = -playerSpeed;
                playerAnimationController = CurrentAnimation.Run;
                effects = SpriteEffects.FlipHorizontally;
            }
            else if (keyboard.IsKeyDown(Keys.D))
            {
                targetVel = playerSpeed;
                playerAnimationController = CurrentAnimation.Run;
                effects = SpriteEffects.None;
            }
            
            if (velocity.X > targetVel)
            {
                velocity.X -= playerSpeed * dt * friction;
                velocity.X = Math.Max(targetVel, velocity.X);
            }
            else
            {
                velocity.X += playerSpeed * dt * friction;
                velocity.X = Math.Min(targetVel, velocity.X);
            }
        }

        private bool IsGrounded()
        {
            Rectangle feetRectangle = new Rectangle(hitbox.X, hitbox.Y + hitbox.Height, hitbox.Width, 3);
            return levelCollisionFunc(feetRectangle).HasValue;
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            Vector2 intPosition = new Vector2((int)position.X, (int)position.Y);
            switch (playerAnimationController)
            {
                case CurrentAnimation.Idle:
                    playerAnimation[0].Draw(spriteBatch, intPosition, gameTime, effects);
                    break;
                case CurrentAnimation.Run:
                    playerAnimation[1].Draw(spriteBatch, intPosition, gameTime, effects);
                    break;
                case CurrentAnimation.Jumping:
                    playerAnimation[2].Draw(spriteBatch, intPosition, gameTime, effects);

                    break;
                case CurrentAnimation.Falling:
                    playerAnimation[3].Draw(spriteBatch, intPosition, gameTime, effects);

                    break;
            }

        }

        public void AddToInventory (InventoryObject obj) => inventory.Add(obj);

        public bool Has<T>() => inventory.Any(obj => obj is T);
    }
}