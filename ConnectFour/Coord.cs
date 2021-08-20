using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConnectFour
{
    public record Coord
    {
        public readonly int X;
        public readonly int Y;

        public Coord(int x, int y)
        {
            X = x;
            Y = y;
        }

        public Coord((int x, int y) pair)
            : this(pair.x, pair.y) { }

        public static Coord operator +(Coord first, Coord second)
            => new (first.X + second.X, first.Y + second.Y);

        public static Coord operator -(Coord coord)
            => new(-coord.X, -coord.Y);

        public static Coord operator -(Coord first, Coord second)
            => first + (-second);

        public static Coord operator *(int scalar, Coord coord)
            => new(scalar * coord.X, scalar * coord.Y);

        public static readonly Coord Zero = new(0, 0);
    }
}
