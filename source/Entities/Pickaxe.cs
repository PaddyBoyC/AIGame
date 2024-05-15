using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source.Entities
{
    internal class Pickaxe : InventoryObject
    {
        public Pickaxe(Vector2 position, Texture2D sprite) :
            base(position, sprite)
        {

        }
    }
}
