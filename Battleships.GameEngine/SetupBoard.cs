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

        public Dictionary<Point, Ship> ShipsByOccupationPoints = new Dictionary<Point, Ship>(17);

        public bool AllSunk => m_ShipsByLength.Values.SelectMany(ss => ss).All(s => s.IsSunk);

        public void AddShip(Ship ship)
        {
            m_ShipsByLength[ship.Length].Add(ship);

            var shipOverlaps = false;
            foreach (var p in ship.Occupies)
            {
                if (ShipsByOccupationPoints.ContainsKey(p))
                    shipOverlaps = true;
                else
                    ShipsByOccupationPoints[p] = ship;
            }


            IsValid = m_ShipsByLength[2].Count == s_LengthShipsRequired[2] &&
                      m_ShipsByLength[3].Count == s_LengthShipsRequired[3] &&
                      m_ShipsByLength[4].Count == s_LengthShipsRequired[4] &&
                      m_ShipsByLength[5].Count == s_LengthShipsRequired[5] &&
                      !shipOverlaps;
        }
    }
}
