using AIGame.source;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
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

        public float playerSpeed = 150;
        public float fallAccel = 1500;
        public float maxFallSpeed = 950;
        public float jumpSpeed = -400;

        public bool isFalling = true;
        public bool isJumping;
        public bool isShooting;

        Animation[] playerAnimation;
        CurrentAnimation playerAnimationController;

        Func<Rectangle, Rectangle?> levelCollisionFunc;


        Vector2 hitboxOffset;

        public Player(Vector2 position, Texture2D idleSprite, Texture2D runSprite, Texture2D jumpSprite, Texture2D fallSprite, Func<Rectangle, Rectangle?> levelCollisionFunc)
        {
            playerAnimation = new Animation[4];

            this.position = position;
            this.levelCollisionFunc = levelCollisionFunc;
            this.position = new Vector2(0, 0);

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
            KeyboardState keyboard = Keyboard.GetState();


            playerAnimationController = CurrentAnimation.Idle;

            isShooting = keyboard.IsKeyDown(Keys.Enter);

            Move(keyboard);
        
            //apply x movement and check for collision
            position.X += velocity.X * dt;
            hitbox.X = (int)position.X + (int)hitboxOffset.X;
            Rectangle? collidingRectangle = levelCollisionFunc(hitbox);
            if(collidingRectangle.HasValue)
            {
                if (velocity.X > 0)
                {
                    hitbox.X = collidingRectangle.Value.X - hitbox.Width - 1;
                }
                else
                {
                    hitbox.X = collidingRectangle.Value.X + collidingRectangle.Value.Width + 1;
                }
                velocity.X = 0;
                position.X = hitbox.X - hitboxOffset.X;
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
            collidingRectangle = levelCollisionFunc(hitbox);
            if (collidingRectangle.HasValue)
            {
                if (velocity.Y > 0)
                {
                    isFalling = false;
                    isJumping = false;
                    hitbox.Y = collidingRectangle.Value.Y - hitbox.Height;
                }
                else
                {
                    hitbox.Y = collidingRectangle.Value.Y +collidingRectangle.Value.Height + 1;
                }
                velocity.Y = 0;                   
                position.Y = hitbox.Y - hitboxOffset.Y;
            }

            if (velocity.Y != 0)
            {
                playerAnimationController = CurrentAnimation.Falling;
            }
        }

        private void Move(KeyboardState keyboard)
        {

            if (keyboard.IsKeyDown(Keys.A))
            {
                velocity.X = -playerSpeed;
                playerAnimationController = CurrentAnimation.Run;
                effects = SpriteEffects.FlipHorizontally;
            }
            else if (keyboard.IsKeyDown(Keys.D))
            {
                velocity.X = playerSpeed;
                playerAnimationController = CurrentAnimation.Run;
                effects = SpriteEffects.None;
            }
            else
            {
                velocity.X = 0;
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
    }
}