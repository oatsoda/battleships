using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;

namespace Battleships.GameEngine
{
    public class Game
    {
        private SetupBoard m_StartBoard;

        public Game(SetupBoard startBoard) 
        { 
            if (!startBoard.IsValid)
                throw new ArgumentException("Start Board is not valid.",  nameof(startBoard));

            m_StartBoard = startBoard;

        }
    }
    
    public class SetupBoard
    {
        private static readonly Dictionary<int, int> s_LengthShipsRequired = new Dictionary<int, int>
        {
            { 2, 1 },
            { 3, 2 },
            { 4, 1 },
            { 5, 1 }
        };

        private Dictionary<int, List<Ship>> m_ShipsByLength = new Dictionary<int, List<Ship>>(
                s_LengthShipsRequired.Select(kvp => new KeyValuePair<int, List<Ship>>(kvp.Key, new List<Ship>(kvp.Value))) // Init empty lists with required capacity
                );

        public bool IsValid { get; private set; }

        public List<Point> OccupationPoints = new List<Point>(17);

        public void AddShip(Ship ship)
        {
            m_ShipsByLength[ship.Length].Add(ship);
            OccupationPoints.AddRange(ship.Occupies);

            IsValid = m_ShipsByLength[2].Count == s_LengthShipsRequired[2] &&
                      m_ShipsByLength[3].Count == s_LengthShipsRequired[3] &&
                      m_ShipsByLength[4].Count == s_LengthShipsRequired[4] &&
                      m_ShipsByLength[5].Count == s_LengthShipsRequired[5] &&
                      !ShipsOverlap();
        }

        private bool ShipsOverlap()
        {
            // TODO: Optimise by pre-populating lists, occupies or by checking separate in AddShip? etc.
            return OccupationPoints.GroupBy(p => p).Any(g => g.Count() > 1);
        }
    }

    public class Ship
    {
        public int Length { get; private set; }
        public bool IsVertical { get; private set; }

        public List<Point> Occupies { get; private set; }

        public Ship(GridSquare start, GridSquare end) : this(start.Point, end.Point) { }

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

            Occupies = IsVertical 
                ? Enumerable.Range(start.Y, (end.Y - start.Y)+1).Select(y => new Point(start.X, y)).ToList()
                : Enumerable.Range(start.X, (end.X - start.X)+1).Select(x => new Point(x, start.Y)).ToList();
        }
    }

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
        
        public static implicit operator GridSquare(string c) => new GridSquare(c);
    }
}
