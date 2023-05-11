using System;
using System.Collections.Generic;

namespace Shared.Game
{
    [Serializable]
    public class GameState
    {
        public const int MapWidth = 16, MapHeight = 16, Dice = 100, SuperChance = 15, FruitValue = 10, SuperFruitValue = 25, MaxFruits = 12;
        public Dictionary<string, Player> Players = new Dictionary<string, Player>();
        public List<Fruit> Fruits = new List<Fruit>();

        public void AddPlayer(string id)
        {
            Random random = new Random();
            Position pos = new Position(random.Next(MapWidth), random.Next(MapHeight));
            
            Players.Add(id, new Player() { Position = pos });
        }
        public void RemovePlayer(string id) => Players.Remove(id);

        public void AddFruit()
        {
            if (Fruits.Count >= MaxFruits) return;

            Random random = new Random();
            Position pos = new Position(random.Next(MapWidth), random.Next(MapHeight));

            Fruits.Add(new Fruit() { Position = pos, IsSuper = random.Next(Dice) <= SuperChance ? true : false });
        }
    }
}