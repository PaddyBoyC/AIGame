using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;

namespace AIGame.source
{
    public abstract class Entity
    {

        public Vector2 position;
        public Rectangle hitbox;

        public abstract void Update(GameTime gameTime);

        public abstract void Draw(SpriteBatch spriteBatch, GameTime gameTime);
    }
}
