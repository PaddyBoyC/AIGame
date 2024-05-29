using AIGame.source.StateMachineNS;
using Apos.Gui;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Content;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AIGame.source
{
    internal class MainMenu
    {
        public bool StartGame { get; private set; } = false;

        SpriteBatch _spriteBatch;
        private RenderTarget2D renderTarget;

        private Texture2D titleImage;

        GraphicsDevice GraphicsDevice;
        public MainMenu(ContentManager Content, GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch, RenderTarget2D renderTarget)           
        {
            _spriteBatch = spriteBatch;
            this.renderTarget = renderTarget;
            this.GraphicsDevice = GraphicsDevice;

            titleImage = Content.Load<Texture2D>("MenuTitle");
        }

        public void Update()
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Space))
            {
                StartGame = true;
            }

            float screenWidth = Game1.screenWidth;
            float screenHeight = Game1.screenHeight;

            Panel.Push().XY = new Vector2(screenWidth / 2 - 130, screenHeight / 2 - 200);

            Label.Put("Press space to start");

            Panel.Pop();
        }

        public void Draw()
        {
            _spriteBatch.Begin();
            _spriteBatch.Draw(titleImage, new Rectangle(0, 0, 64*4, 64*4), Color.White);
            _spriteBatch.End();
        }
    }
}
