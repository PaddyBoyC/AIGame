using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using MonoGame.Extended.Sprites;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source
{
    public class InventoryObject : Entity
    {
        Texture2D sprite;

        public InventoryObject(Vector2 position, Texture2D sprite)
        {
            this.position = position;
            this.sprite = sprite;  
            this.hitbox = new Rectangle((int)position.X, (int)position.Y, 32, 32);

        }

        public override void Update(GameTime gameTime)
        {
            
        }

        public override void Draw(SpriteBatch spriteBatch, GameTime gameTime)
        {
            spriteBatch.Draw(sprite, position, null, Color.White, 0f, new Vector2(), 1f, SpriteEffects.None, 1);
        }
    }
}

