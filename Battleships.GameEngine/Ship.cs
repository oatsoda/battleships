using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleships.GameEngine
{
    public class Ship
    {
        private Dictionary<Point, bool> m_PointsHit;

        public int Length { get; private set; }
        public bool IsVertical { get; private set; }
        
        public IEnumerable<Point> Occupies => m_PointsHit.Keys;

        public Ship(GridSquare start, GridSquare end) : this(start.Point, end.Point) { }

        public bool IsSunk { get; private set; }

        private Ship(Point start, Point end)
        {
            if (start.X != end.X && start.Y != end.Y)
                throw new ArgumentException("Ships cannot be diagonal. X or Y of Start and End must be the same.");
           
            var isVertical = start.X == end.X;
            if ((isVertical && start.Y > end.Y) || (!isVertical && start.X > end.X))
            {
                var c = start;
                start = end;
                end = c;
            }

            var length = (isVertical 
                ? end.Y - start.Y
                : end.X - start.X) + 1;
            
            if (length < 2 || length > 5)
                throw new ArgumentException($"Ships length must be between 2 and 5. Length was {length}");

            IsVertical = isVertical;
            Length = length;

            m_PointsHit = IsVertical 
                ? Enumerable.Range(start.Y, (end.Y - start.Y)+1).Select(y => new Point(start.X, y)).ToDictionary(p => p, p => false)
                : Enumerable.Range(start.X, (end.X - start.X)+1).Select(x => new Point(x, start.Y)).ToDictionary(p => p, p => false);
        }

        internal bool Hit(Point point)
        {
            m_PointsHit[point] = true;
            IsSunk = m_PointsHit.All(p => p.Value); // Whether sunk
            return IsSunk;
        }
        
        public static implicit operator Ship((string start, string end) coords) => new Ship(coords.start, coords.end);
    }
}
