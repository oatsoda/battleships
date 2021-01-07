using System;
using System.Drawing;
using System.Text.RegularExpressions;

namespace Battleships.GameEngine
{
    public struct GridSquare
    {
        private static readonly Regex s_Reg = new Regex("^[A-Ja-j]{1}[0-9]{1}$");

        public char X { get; }
        public byte Y { get; }
        public Point Point { get; }

        public GridSquare(string coordinate)
        {
            if (!s_Reg.IsMatch(coordinate))
                throw new ArgumentException($"Coordinate must be two characters A-J followed by 0-9. '{coordinate}'");

            X = char.ToUpper(coordinate[0]);
            Y = byte.Parse(coordinate[1].ToString());
            Point = new Point(X - 65, Y);
        }

        internal GridSquare(Point point)
        {
            Point = point;
            X = (char)(point.X+65);
            Y = (byte)Point.Y;
        }
        
        public static implicit operator GridSquare(string c) => new GridSquare(c);

        public override string ToString() => $"{X}{Y}";
    }
}
