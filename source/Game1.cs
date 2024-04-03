using Apos.Gui;
using FontStashSharp;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using System.Collections.Generic;
using System;
using TiledSharp;
using AIGame.source.Flocking;
using AIGame.source.StateMachineNS;
using AIGame.source.States;

namespace AIGame.source
{
    public class Game1 : Game
    {
        private GraphicsDeviceManager _graphics;
        private SpriteBatch _spriteBatch;
        private RenderTarget2D renderTarget;

        public static float screenWidth;
        public static float screenHeight;

        private PlayingGame game;
        private MainMenu mainMenu;

        IMGUI _ui;
        private enum State
        {
            MainMenu,
            PlayingGame,
        }

        State state = State.MainMenu;

        public static GameTime LastGameTime { get; private set; } = null;

        public Game1()
        {
            _graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";
            IsMouseVisible = true;
        }

        protected override void Initialize()
        {
            _graphics.PreferredBackBufferHeight = 930;
            _graphics.PreferredBackBufferWidth = 1024;
            _graphics.ApplyChanges();
            screenHeight = _graphics.PreferredBackBufferHeight;
            screenWidth = _graphics.PreferredBackBufferWidth;
            base.Initialize();
            _ui = new IMGUI();
            //create attract mode game
            game = new PlayingGame(Content, GraphicsDevice, _spriteBatch, attractMode: true);
        }

        protected override void LoadContent()
        {
            _spriteBatch = new SpriteBatch(GraphicsDevice);

            #region UI
            FontSystem fontSystem = FontSystemFactory.Create(GraphicsDevice, 2048, 2048);
            fontSystem.AddFont(TitleContainer.OpenStream($"{Content.RootDirectory}/PixeloidSansBold-PKnYd.ttf"));
            GuiHelper.Setup(this, fontSystem);
            #endregion
            
            renderTarget = new RenderTarget2D(GraphicsDevice, 960, 850);

            mainMenu = new MainMenu(Content, GraphicsDevice, _spriteBatch, renderTarget);
        }

        protected override void Update(GameTime gameTime)
        {

            #region UI
            GuiHelper.UpdateSetup(gameTime);
            _ui.UpdateAll(gameTime);
            #endregion

            LastGameTime = gameTime;
            switch (state)
            {
                case State.MainMenu:
                    {
                        game.Update();
                        mainMenu.Update();
                        if (mainMenu.StartGame)
                        {
                            game = new PlayingGame(Content, GraphicsDevice, _spriteBatch);
                            state = State.PlayingGame; 
                        }
                        break;
                    }
                case State.PlayingGame:
                    {
                        game.Update();
                        if (game.Reset)
                        {
                            mainMenu = new MainMenu(Content, GraphicsDevice, _spriteBatch, renderTarget);
                            game = new PlayingGame(Content, GraphicsDevice, _spriteBatch, attractMode: true);
                            state = State.MainMenu;
                        }
                        break;
                    }
            }
            GuiHelper.UpdateCleanup();
        }      

        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.SetRenderTarget(renderTarget);

            switch (state)
            {
                case State.MainMenu:
                    {
                        game.Draw(gameTime);
                        mainMenu.Draw();
                        break;
                    }
                case State.PlayingGame:
                    {
                        game.Draw(gameTime);
                        break;
                    }
            }
            GraphicsDevice.SetRenderTarget(null);

            _spriteBatch.Begin(samplerState: SamplerState.PointClamp);
            _spriteBatch.Draw(renderTarget, new Vector2(0, 0), null, Color.White, 0f, new Vector2(), 2f, SpriteEffects.None, 0);
            _spriteBatch.End();

            _ui.Draw(gameTime);
            base.Draw(gameTime);
        }
    }   
}