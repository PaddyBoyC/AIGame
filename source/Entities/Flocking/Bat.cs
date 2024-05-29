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
    internal class Bat : Bird
    {
        public Bat(Vector2 position, (Texture2D, Texture2D) spriteSheets, List<Bird> bats, Player player, Vector2? biasDirection = null):
            base(position, spriteSheets, bats, player, biasDirection, 80, 0, 532, 900, 860)
        {
            
        }
    }
}



