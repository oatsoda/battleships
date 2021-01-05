using Battleships.GameEngine.Random;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleships.GameEngine
{
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

        public Dictionary<Point, Ship> ShipsByOccupationPoints { get; } = new Dictionary<Point, Ship>(17);

        public bool AllSunk => m_ShipsByLength.Values.SelectMany(ss => ss).All(s => s.IsSunk);

        public void AddShip(Ship ship)
        {
            // This checks for overlap but does not prevent
            var shipOverlaps = ShipOverlaps(ship);
            RecordShip(ship, shipOverlaps);
        }

        private void RecordShip(Ship ship, bool shipOverlaps)
        {
            RecordOccupationPoints(ship);
            m_ShipsByLength[ship.Length].Add(ship);

            IsValid = m_ShipsByLength[2].Count == s_LengthShipsRequired[2] &&
                      m_ShipsByLength[3].Count == s_LengthShipsRequired[3] &&
                      m_ShipsByLength[4].Count == s_LengthShipsRequired[4] &&
                      m_ShipsByLength[5].Count == s_LengthShipsRequired[5] &&
                      !shipOverlaps;
        }
        
        private bool ShipOverlaps(Ship ship)
        {
            return ship.Occupies.Any(p => ShipsByOccupationPoints.ContainsKey(p));
        }

        private void RecordOccupationPoints(Ship ship)
        {
            foreach (var p in ship.Occupies)
                ShipsByOccupationPoints[p] = ship;
        }

        public SetupBoard GenerateRandom()
        {
            if (m_ShipsByLength.Values.Any(ss => ss.Count > 0))
                throw new InvalidOperationException("Ships have already been added manually.");

            var coordGen = new RandomCoordGenerator();

            foreach (var req in s_LengthShipsRequired)
            {
                for (int i = 1; i <= req.Value; i++)
                {                    
                    var ship = GenerateValidShipOfLength(req.Key, coordGen);
                    RecordShip(ship, false);
                }
            }

            return this;
        }

        private Ship GenerateValidShipOfLength(int length, RandomCoordGenerator random)
        {
            Ship ship;
            do
            {
                // Prevent if overlap
                ship = GenerateShipOfLength(length, random);
            }
            while (ShipOverlaps(ship));
            return ship;
        }

        private static Ship GenerateShipOfLength(int length, RandomCoordGenerator random)
        {
            var (start, end) = random.GetRandomShipCoords(length);
            return new Ship(start, end);
        }
    }
}
