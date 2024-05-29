using AIGame.source.Entities;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source.Flocking
{
    internal class SnowBat : Bird
    {
        public SnowBat(Vector2 position, (Texture2D, Texture2D) spriteSheets, List<Bird> snowbats, Player player, Vector2? biasDirection = null) :
            base(position, spriteSheets, snowbats, player, biasDirection, 80, 0, 990, 900, 1376)
        {

        }
    }
}
