using System;
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using Microsoft.Xna.Framework;
using Newtonsoft.Json;

namespace Shared
{
    [Serializable]
    public class GameState
    {
        public static Action<string> AFKAction;
        public Dictionary<string, Player> Players = new Dictionary<string, Player>();
        public Dictionary<string, Fruit> Fruits = new Dictionary<string, Fruit>();

        private const int MaxFruits = 10, FruitValue = 5, SuperFruitValue = 10;
        private const float TimeToSpawn = 5;
        private static Random Random = new Random();
        private float spawnTimer = 0;

        public void AddPlayer(IPEndPoint endPoint)
        {
            if (!Players.ContainsKey(endPoint.Stringfy())) Players.Add(endPoint.Stringfy(), new Player(Random));
        }
        public void RemovePlayer(string id)
        {
            if (Players.ContainsKey(id)) Players.Remove(id);
        }

        public void Update(float deltaTime)
        {
            if (spawnTimer >= TimeToSpawn)
            {
                spawnTimer = 0;
                string fruitId = Guid.NewGuid().ToString();

                if (!Fruits.ContainsKey(fruitId)) Fruits.Add(fruitId, new Fruit(Random));
            }
            else spawnTimer += deltaTime;

            List<string> fruitsToDelete = new List<string>(), playersToDelete = new List<string>();

            foreach (KeyValuePair<string, Player> player in Players)
            {
                player.Value.Update(deltaTime, player.Key, ref playersToDelete);

                foreach (KeyValuePair<string, Fruit> fruit in Fruits)
                {
                    foreach (Rectangle collider in player.Value.Colliders)
                    {
                        if (fruit.Value.Collider.Intersects(collider))
                        {
                            player.Value.Score += fruit.Value.IsSuper ? SuperFruitValue : FruitValue;

                            fruitsToDelete.Add(fruit.Key);

                            break;
                        }
                    }
                }
            }

            foreach (string delete in fruitsToDelete) Fruits.Remove(delete);

            foreach (string delete in playersToDelete) AFKAction(delete);
        }
    }
    [Serializable]
    public class Player
    {
        public const float Speed = Constants.Game.EntitySize * 6, AFKTolerance = 60;
        public int Score;
        public Vector2 Direction, Position;
        [JsonIgnore] public List<Rectangle> Colliders
        {
            get
            {
                Rectangle area = new Rectangle(Position.ToPoint(), new Point(Constants.Game.EntitySize));
                List<Rectangle> rects = new List<Rectangle>();

                rects.GetWarp(area, new Rectangle(Point.Zero, new Point(Constants.Game.MapSize)));
                rects.Add(area);

                return rects;
            }
        }
        private float afkTimer = 0;

        public Player() { }
        public Player(Random random) => Position = new Vector2(random.Next(Constants.Game.MapSize - Constants.Game.EntitySize), random.Next(Constants.Game.MapSize - Constants.Game.EntitySize));

        public void Update(float deltaTime, string id, ref List<string> removeList)
        {
            Position += Direction * Speed * deltaTime;

            if (Position.X >= Constants.Game.MapSize) Position.X = 0;
            else if (Position.X <= -Constants.Game.EntitySize) Position.X = Constants.Game.MapSize - 1 - Constants.Game.EntitySize;

            if (Position.Y >= Constants.Game.MapSize) Position.Y = 0;
            else if (Position.Y <= -Constants.Game.EntitySize) Position.Y = Constants.Game.MapSize - 1 - Constants.Game.EntitySize;

            if (Direction != Vector2.Zero) afkTimer = 0;
            else
            {
                afkTimer += deltaTime;

                if (afkTimer >= AFKTolerance) removeList.Add(id);
            }
        }
    }
    [Serializable]
    public class Fruit
    {
        public const int Dice = 1000;
        public const float MaxToSuper = .1f;
        public bool IsSuper;
        public Vector2 Position;
        [JsonIgnore] public Rectangle Collider => new Rectangle(Position.ToPoint(), new Point(Constants.Game.EntitySize));

        public Fruit() { }
        public Fruit(Random random)
        {
            Position = new Vector2(random.Next(Constants.Game.MapSize - Constants.Game.EntitySize), random.Next(Constants.Game.MapSize - Constants.Game.EntitySize));
            IsSuper = random.Next(Dice) <= Dice * MaxToSuper;
        }
    }
}