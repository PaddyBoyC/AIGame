using Microsoft.Xna.Framework;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source
{
    public class GameManager
    {
        private Rectangle engRectangle;
        public GameManager(Rectangle endRectangle)
        {
            this.engRectangle = endRectangle;
        }

        public bool HasGameEnded(Rectangle playerHitbox)
        {
            return engRectangle.Intersects(playerHitbox);
        }
    }
}
