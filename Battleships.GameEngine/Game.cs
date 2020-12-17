﻿using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

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

        public SetupBoard()
        {
        }

        public void AddShip(Ship ship)
        {
            m_ShipsByLength[ship.Length].Add(ship);

            IsValid = m_ShipsByLength[2].Count == s_LengthShipsRequired[2] &&
                      m_ShipsByLength[3].Count == s_LengthShipsRequired[3] &&
                      m_ShipsByLength[4].Count == s_LengthShipsRequired[4] &&
                      m_ShipsByLength[5].Count == s_LengthShipsRequired[5];
        }

        public bool IsValid { get; private set; }
    }

    public class Ship
    {
        public int Length { get; private set; }
        public bool IsVertical { get; private set; }

        public Ship(Point start, Point end)
        {
            if (start.X != end.X && start.Y != end.Y)
                throw new ArgumentException("Ships cannot be diagonal. X or Y of Start and End must be the same.");
           
            var isVertical = start.X == end.X;
            var length = isVertical 
                ? end.Y - start.Y
                : end.X - start.X; // TODO: Disallow ends < start etc.
            
            if (length < 2 || length > 5)
                throw new ArgumentException("Ships length must be between 2 and 5.");

            IsVertical = isVertical;
            Length = length;
        }
    }
}
