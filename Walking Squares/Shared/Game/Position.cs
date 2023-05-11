using System;

namespace Shared.Game
{
    [Serializable]
    public struct Position
    {
        public int X, Y;

        public Position(int value) : this(value, value) { }
        public Position(int x, int y)
        {
            X = x;
            Y = y;
        }

        public override bool Equals(object obj)
        {
            if (!(obj is Position position)) return false;

            return X == position.X && Y == position.Y;
        }

        public override int GetHashCode()
        {
            int hashCode = 1861411795;
            hashCode = hashCode * -1521134295 + X.GetHashCode();
            hashCode = hashCode * -1521134295 + Y.GetHashCode();

            return hashCode;
        }

        public static bool operator ==(Position a, Position b) => a.X == b.X && a.Y == b.Y;
        public static bool operator !=(Position a, Position b) => a.X != b.X || a.Y != b.Y;
    }
}