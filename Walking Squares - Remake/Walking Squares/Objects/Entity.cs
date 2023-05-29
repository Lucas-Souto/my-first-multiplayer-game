using System;
using System.Collections.Generic;
using Microsoft.Xna.Framework;
using Microsoft.Xna.Framework.Graphics;
using Microsoft.Xna.Framework.Input;
using Shared;

namespace WS.Objects
{
    public class Entity : DrawableGameComponent
    {
        public Vector2 Position;
        public Color Color = Color.White;

        public Entity() : base(Main.Game) => Main.Game.Components.Add(this);

        public override void Draw(GameTime gameTime)
        {
            Rectangle area = new Rectangle(Position.ToPoint(), new Point(Constants.Game.EntitySize));
            List<Rectangle> rects = new List<Rectangle>();

            rects.GetWarp(area, new Rectangle(Point.Zero, new Point(Constants.Game.MapSize)));
            rects.Add(area);

            foreach (Rectangle rect in rects) Main.Game.SpriteBatch.Draw(Main.Pixel, rect.Location.ToVector2(), new Rectangle(Point.Zero, rect.Size), Color);
        }
    }
}