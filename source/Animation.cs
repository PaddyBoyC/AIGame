﻿using Microsoft.Xna.Framework;
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
        float timeSinceLastFrame = 0;

        public Animation(Texture2D spritesheet, float width = 32, float height = 32)
        {
            this.spritesheet = spritesheet;
            frames = (int)(spritesheet.Width / width); 
        }

        public void Draw(SpriteBatch spriteBatch, Vector2 position, GameTime gameTime, float millisecondsperframes = 500, SpriteEffects effect = SpriteEffects.None)
        {
            if (c < frames)
            {
                var rect = new Rectangle(32 * c, rows, 32, 32);
                spriteBatch.Draw(spritesheet, position, rect, Color.White, 0f, new Vector2(), 1f, effect, 1);
                timeSinceLastFrame += (float)gameTime.ElapsedGameTime.TotalMilliseconds;
                if (timeSinceLastFrame > millisecondsperframes)
                {
                    timeSinceLastFrame -= millisecondsperframes;
                    c++;
                    if (c == frames)
                    {
                        c = 0;
                    }
                }
            }
            
        }
    }
}
