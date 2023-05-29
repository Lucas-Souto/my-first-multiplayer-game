using System.Collections.Generic;
using Microsoft.Xna.Framework;

namespace WS.Objects
{
    public class FruitPuppet : Entity
    {
        private string id;
        public static Dictionary<string, FruitPuppet> Fruits = new Dictionary<string, FruitPuppet>();

        public FruitPuppet(bool super, string id)
        {
            this.id = id;
            Color = super ? Color.Red : Color.Green;
            DrawOrder = 1;

            Fruits.Add(id, this);
        }

        public void Remove()
        {
            Fruits.Remove(id);
            Game.Components.Remove(this);
        }

        public override void Update(GameTime gameTime)
        {
            base.Update(gameTime);

            if (!Main.Game._client.Game.Fruits.ContainsKey(id)) Remove();
        }
    }
}