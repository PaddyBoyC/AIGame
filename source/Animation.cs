using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source
{
    public class Animation
    {
        Texture2D spritesheet;
        int frames;
        int rows = 0;
        int c = 0;
        int width, height;
        float timeSinceLastFrame = 0;
        float millisecondsPerFrame;
        public bool Playing { get; set; } = true;
        public bool Loop { get; set; } = true;

        public void Reset() => c = 0;

        public Animation(Texture2D spritesheet, int width = 32, int height = 32, float millisecondsPerFrame = 500)
        {
            this.spritesheet = spritesheet;
            frames = (int)(spritesheet.Width / width);
            this.width = width;
            this.height = height;
            this.millisecondsPerFrame = millisecondsPerFrame;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, GameTime gameTime, SpriteEffects effect = SpriteEffects.None)
        {
            if (c < frames)
            {
                var rect = new Rectangle(width * c, rows, width, height);
                spriteBatch.Draw(spritesheet, position, rect, Color.White, 0f, new Vector2(), 1f, effect, 1);
                if (Playing)
                {
                    timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                    if (timeSinceLastFrame > millisecondsPerFrame)
                    {
                        timeSinceLastFrame -= millisecondsPerFrame;
                        c++;
                        if (c == frames)
                        {
                            c = Loop ? 0 : frames - 1;
                        }
                    }
                }              
            }
            
        }

        public bool AtEnd()
        {
            return c == frames - 1;
        }
    }
}
