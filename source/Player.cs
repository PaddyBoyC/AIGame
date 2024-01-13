﻿using AIGame.source;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
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
        public Rectangle playerFallRect;
        public SpriteEffects effects;

        public float playerSpeed = 2;
        public float fallSpeed = 3f;
        public float jumpSpeed = -10;
        public float startY;

        public bool isFalling = true;
        public bool isJumping;
        public bool isShooting;

        public Animation[] playerAnimation;
        public CurrentAnimation playerAnimationController;

        public Player(Vector2 position, Texture2D idleSprite, Texture2D runSprite, Texture2D jumpSprite, Texture2D fallSprite)
        {
            playerAnimation = new Animation[4];

            this.position = position;
            velocity = new Vector2();
            effects = SpriteEffects.None;

            playerAnimation[0] = new Animation(idleSprite, millisecondsPerFrame: 150);
            playerAnimation[1] = new Animation(runSprite, millisecondsPerFrame: 100);
            playerAnimation[2] = new Animation(jumpSprite, millisecondsPerFrame: 100);
            playerAnimation[3] = new Animation(fallSprite, millisecondsPerFrame: 600);

            hitbox = new Rectangle((int)position.X, (int)position.Y, 32, 25);
            playerFallRect = new Rectangle((int)position.X + 3, (int)position.Y + 32, 32, (int)fallSpeed);
        }
        public override void Update()
        {
            KeyboardState keyboard = Keyboard.GetState();


            playerAnimationController = CurrentAnimation.Idle;
            position = velocity;

            isShooting = keyboard.IsKeyDown(Keys.Enter);

            startY = position.Y;
            Move(keyboard);
            Jump(keyboard);

            if (isFalling)
            {
                velocity.Y += fallSpeed;
                playerAnimationController = CurrentAnimation.Falling;
            }
            hitbox.X = (int)position.X;
            hitbox.Y = (int)position.Y;
            playerFallRect.X = (int)position.X;
            playerFallRect.Y = (int)(position.Y + 34);
        }
        private void Move(KeyboardState keyboard)
        {

            if (keyboard.IsKeyDown(Keys.A))
            {
                velocity.X -= playerSpeed;
                playerAnimationController = CurrentAnimation.Run;
                effects = SpriteEffects.FlipHorizontally;
            }
            if (keyboard.IsKeyDown(Keys.D))
            {
                velocity.X += playerSpeed;
                playerAnimationController = CurrentAnimation.Run;
                effects = SpriteEffects.None;
            }
        }
        private void Jump(KeyboardState keyboard)
        {
            if (isJumping)
            {
                velocity.Y += jumpSpeed;//Making it go up
                jumpSpeed += 1f;
                Move(keyboard);
                playerAnimationController = CurrentAnimation.Jumping;

                if (velocity.Y >= startY)
                //If it's farther than ground
                {
                    velocity.Y = startY;
                    isJumping = false;

                }
            }
            else
            {
                if (keyboard.IsKeyDown(Keys.Space) && !isFalling)
                {
                    isJumping = true;
                    isFalling = false;
                    jumpSpeed = -10;//Give it upward thrust
                }
            }

        }
        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {

            switch (playerAnimationController)
            {
                case CurrentAnimation.Idle:
                    playerAnimation[0].Draw(spriteBatch, position, gameTime, effects);
                    break;
                case CurrentAnimation.Run:
                    playerAnimation[1].Draw(spriteBatch, position, gameTime, effects);
                    break;
                case CurrentAnimation.Jumping:
                    playerAnimation[2].Draw(spriteBatch, position, gameTime, effects);

                    break;
                case CurrentAnimation.Falling:
                    playerAnimation[3].Draw(spriteBatch, position, gameTime, effects);

                    break;
            }

        }
    }
}