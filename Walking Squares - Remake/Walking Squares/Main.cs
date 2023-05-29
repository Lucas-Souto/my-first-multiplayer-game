using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared;
using WS.Objects;

namespace WS
{
    public class Main : Game
    {
        public static Main Game;
        public static Texture2D Pixel;
        public static SpriteFont Font;

        public SpriteBatch SpriteBatch;
        private GraphicsDeviceManager graphics;

        public Client _client;

        private const float TimeToReconect = 2;
        private float reconectTimer = TimeToReconect, afkTimer = 0;

        public Main()
        {
            Game = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = Constants.Game.MapSize;
            graphics.PreferredBackBufferHeight = Constants.Game.MapSize;
        }
        
        protected override void Initialize()
        {
            Pixel = new Texture2D(GraphicsDevice, 1, 1);
            Color[] color = new Color[] { Color.White };

            Pixel.SetData(color);

            base.Initialize();
        }
        
        protected override void LoadContent()
        {
            SpriteBatch = new SpriteBatch(GraphicsDevice);
            Font = Content.Load<SpriteFont>("Font");

            base.LoadContent();
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            _client?.Close();
            base.OnExiting(sender, args);
        }

        protected override void Update(GameTime gameTime)
        {
            float deltaTime = (float)gameTime.ElapsedGameTime.TotalSeconds;

            Input.Update();

            if (_client == null) _client = new Client();
            else
            {
                if (!_client.Connected)
                {
                    if (reconectTimer >= TimeToReconect)
                    {
                        Console.WriteLine("Tentando conectar...");
                        _client.Send(new Message(MessageType.Login, new Dictionary<string, object>()));

                        reconectTimer = 0;
                    }
                    else reconectTimer += deltaTime;
                }
                else
                {
                    int right = Input.IsKeyDown(Keys.Right) ? 1 : 0, left = Input.IsKeyDown(Keys.Left) ? 1 : 0,
                        up = Input.IsKeyDown(Keys.Up) ? 1 : 0, down = Input.IsKeyDown(Keys.Down) ? 1 : 0;
                    Vector2 direction = new Vector2(right - left, down - up);

                    _client.Send(new Message(MessageType.PlayerInput, new Dictionary<string, object>() { { "direction", direction } }));

                    if (direction != Vector2.Zero) afkTimer = 0;
                    else
                    {
                        afkTimer += deltaTime;

                        if (afkTimer >= Player.AFKTolerance) Exit();
                    }
                }
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.Gray);

            SpriteBatch.Begin(samplerState: SamplerState.PointWrap);
            base.Draw(gameTime);
            SpriteBatch.End();
        }
    }
}