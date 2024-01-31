using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source
{
    public class Door : Entity
    {
        private Animation animation;
        private Player player;
        public bool Opening { get; set; } = false;

        public Door(Animation animation, Rectangle hitbox, Player player)
        {
            this.animation = animation;

            position = new Vector2(hitbox.X, hitbox.Y);
            base.hitbox = hitbox;

            this.player = player;
        }

        public override void Update(GameTime gameTime)
        {
        }

        public void Unlock()
        {
            Opening = true;
            animation.Playing = true;
            player.FreezeTimer = 3;
            if (player.FreezeTimer > 0)
            {
                //animation switches to idle animation (CurrentAnimation.Idle)
            }
        }

        public bool hasHit(Rectangle playerRect)
        {
            if (Opening && animation.AtEnd())
            {
                return false;
            }
            return hitbox.Intersects(playerRect);
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            animation.Draw(spriteBatch, position, gameTime);
        }
    }
}
