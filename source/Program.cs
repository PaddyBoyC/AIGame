using Microsoft.Xna.Framework;
using System;

namespace AIGame.source
{
    public static class Program
    {
        static void Main()
        {
            using (var game = new Game1())
                game.Run();
        }
    }
}
