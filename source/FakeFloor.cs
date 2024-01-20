using AIGame.source;
using AIGame.source.States;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIGame.source
{
    public class FakeFloor : Entity
    {
        private Texture2D texture;      
        private Player player;
        private float yVel = 0;
        private bool falling = false;

        public FakeFloor(Texture2D texture, Rectangle hitbox, Player player)
        {
            this.texture = texture;

            position = new Vector2(hitbox.X, hitbox.Y);
            base.hitbox = hitbox;

            this.player = player;
        }

        public override void Update(GameTime gameTime)
        {
            //falling = true;
            if (falling)
            {
                float dt = (float)gameTime.ElapsedGameTime.TotalSeconds;
                yVel += dt * 100;
                position.Y += yVel * dt;
                hitbox.Y = (int)position.Y;

            }
        }

        public bool hasHit(Rectangle playerRect)
        {
            return hitbox.Intersects(playerRect);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(texture, position, null, Color.White, 0f, new Vector2(), 1f, SpriteEffects.None, 1);
        }
    }
}
