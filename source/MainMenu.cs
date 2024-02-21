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

        IMGUI _ui;
        SpriteBatch _spriteBatch;
        private RenderTarget2D renderTarget;


        GraphicsDevice GraphicsDevice;

        public MainMenu(ContentManager Content, GraphicsDevice GraphicsDevice, SpriteBatch spriteBatch, RenderTarget2D renderTarget)           
        {
            _spriteBatch = spriteBatch;
            this.renderTarget = renderTarget;
            this.GraphicsDevice = GraphicsDevice;

            _ui = new IMGUI();
        }

        public void Update()
        {
            KeyboardState keyboard = Keyboard.GetState();
            if (keyboard.IsKeyDown(Keys.Space))
            {
                StartGame = true;
            }
        }

        public void Draw()
        {
            GraphicsDevice.SetRenderTarget(renderTarget);
            GraphicsDevice.Clear(Color.Green);
            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(renderTarget, new Vector2(0, 0), null, Color.White, 0f, new Vector2(), 2f, SpriteEffects.None, 0);
            _spriteBatch.End();
        }
    }
}
