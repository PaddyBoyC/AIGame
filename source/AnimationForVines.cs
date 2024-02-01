using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source
{
    public class AnimationForVines
    {
        Texture2D spritesheet;
        int frames;
        int rows = 0;
        int c = 0;
        float timeSinceLastFrame = 0;
        float millisecondsPerFrame;
        public bool Playing { get; set; } = true;
        public bool Loop { get; set; } = true;

        public AnimationForVines(Texture2D spritesheet, float width = 48, float height = 48, float millisecondsPerFrame = 500)
        {
            this.spritesheet = spritesheet;
            frames = (int)(spritesheet.Width / width);
            this.millisecondsPerFrame = millisecondsPerFrame;
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, GameTime gameTime, SpriteEffects effect = SpriteEffects.None)
        {
            if (c < frames)
            {
                var rect = new Rectangle(48 * c, rows, 48, 48);
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
