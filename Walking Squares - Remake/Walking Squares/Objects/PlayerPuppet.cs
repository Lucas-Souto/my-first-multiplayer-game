using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Shared;

namespace WS.Objects
{
    public class PlayerPuppet : Entity
    {
        public int Score = 0;
        private string id;
        public static Dictionary<string, PlayerPuppet> Players = new Dictionary<string, PlayerPuppet>();

        public PlayerPuppet(string id, string localId)
        {
            this.id = id;
            Color = id == localId ? Color.Yellow : Color.Black * .75f;
            DrawOrder = id == localId ? 3 : 2;

            Players.Add(id, this);
        }

        public void Remove()
        {
            Main.Game.Components.Remove(Players[id]);
            Players.Remove(id);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Main.Game._client.Game.Players.ContainsKey(id)) Remove();
        }

        public override void Draw(GameTime gameTime)
        {
            base.Draw(gameTime);

            Main.Game.SpriteBatch.DrawString(Main.Font, Score.ToString(), Position + new Vector2(Constants.Game.EntitySize * .5f, -Constants.Game.EntitySize * .5f), Color,  0, Main.Font.MeasureString(Score.ToString()) * .5f, .16f, SpriteEffects.None, 0);
        }
    }
}