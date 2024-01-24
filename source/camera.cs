using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Text;

namespace AIGame.source
{
    public class camera
    {
        public Matrix Transform;

        public Matrix Follow(Rectangle target)
        {
            target.X = MathHelper.Clamp(target.X, (int)Game1.screenWidth / 2 - 270, (int)Game1.screenWidth / 2 + 170);
            //target.Y = MathHelper.Clamp(target.Y, (int)Game1.screenHeight / 4, (int)(Game1.screenHeight * 0.75f));

            if (target.Y < 500)
            {
                target.Y = 220;
            }
            else if (target.Y > 500 && target.Y < 900)
            {
                target.Y = 700;
            }
            else if (target.Y > 900 && target.Y < 1500)
            {
                target.Y = 1180;
            }
        Vector3 translation = new Vector3(-target.X -target.Width/2,
                                       -target.Y - target.Height/2, 0);

            Vector3 offset = new Vector3(Game1.screenWidth/4, (int)(Game1.screenHeight/4), 0);

            Transform = Matrix.CreateTranslation(translation) * Matrix.CreateTranslation(offset);

            return Transform;
        }
    }
}
