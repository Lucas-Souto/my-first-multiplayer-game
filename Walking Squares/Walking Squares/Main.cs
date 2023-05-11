using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared.Game;

namespace Walking_Squares
{
    public class Main : Game
    {
        public static Game Game;
        private bool loaded;
        private GraphicsDeviceManager graphics;
        private SpriteBatch spriteBatch;
        private ClientController client;

        private static Color FruitColor = Color.Green, SuperFruitColor = Color.Red, PlayerColor = Color.Yellow, OthersColor = Color.Black * .25f;
        private static Texture2D Pixel;
        private static SpriteFont Font;
        private const int GridSize = 32;

        public Main()
        {
            Game = this;
            graphics = new GraphicsDeviceManager(this);
            Content.RootDirectory = "Content";

            graphics.PreferredBackBufferWidth = GameState.MapWidth * GridSize;
            graphics.PreferredBackBufferHeight = GameState.MapHeight * GridSize;
        }

        protected override void OnExiting(object sender, EventArgs args)
        {
            if (client != null && client.IsConnected) client.Close();

            base.OnExiting(sender, args);
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
            spriteBatch = new SpriteBatch(GraphicsDevice);

            Font = Content.Load<SpriteFont>("font");
        }
        
        protected override void Update(GameTime gameTime)
        {
            Input.Update();

            if (!loaded)
            {
                loaded = true;
                client = new ClientController();

                client.Start();
            }
            else if (client.IsConnected)
            {
                if (Input.IsKeyPressed(Keys.Escape)) client.Close();
                else
                {
                    int right = Input.IsKeyPressed(Keys.Right) ? 1 : 0,
                        left = Input.IsKeyPressed(Keys.Left) ? 1 : 0,
                        up = Input.IsKeyPressed(Keys.Up) ? 1 : 0,
                        down = Input.IsKeyPressed(Keys.Down) ? 1 : 0;

                    client.Move(new Point(right - left, down - up));
                }
            }

            base.Update(gameTime);
        }
        
        protected override void Draw(GameTime gameTime)
        {
            GraphicsDevice.Clear(Color.LightGray);

            spriteBatch.Begin(samplerState: SamplerState.PointWrap);
            
            Vector2 drawPos;
            Color drawColor;

            foreach (Fruit fruit in client.CurrentState.Fruits)
            {
                drawPos = new Vector2(fruit.Position.X, fruit.Position.Y);
                drawColor = fruit.IsSuper ? SuperFruitColor : FruitColor;

                spriteBatch.Draw(Pixel, drawPos * GridSize, new Rectangle(Point.Zero, new Point(GridSize)), drawColor);
            }

            foreach (KeyValuePair<string, Player> player in client.CurrentState.Players)
            {
                drawPos = new Vector2(player.Value.Position.X, player.Value.Position.Y);
                drawColor = player.Key == client.PlayerId ? PlayerColor : OthersColor;

                spriteBatch.Draw(Pixel, drawPos * GridSize, new Rectangle(Point.Zero, new Point(GridSize)), drawColor);
                spriteBatch.DrawString(Font, player.Value.Score.ToString(), drawPos * GridSize + new Vector2(GridSize * .5f, GridSize + GridSize * .5f), drawColor, 0, Font.MeasureString(player.Value.Score.ToString()) * .5f, GridSize * .5f / 100, SpriteEffects.None, 0);
            }

            spriteBatch.End();

            base.Draw(gameTime);
        }
    }
}
