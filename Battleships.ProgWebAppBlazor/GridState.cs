using Battleships.GameEngine;
using System.Collections.Generic;
using System.Drawing;

namespace Battleships.ProgWebAppBlazor
{
    public class GridState
    {
        public Dictionary<Point, CellState> Grid { get; } 

        public GridState()
        {
            Grid = new Dictionary<Point, CellState>();
            Clear();                        
        }

        public void DrawShip(Ship ship)
        {
            foreach (var s in ship.Occupies)
            {
                Grid[s] = ship.IsSunk ? CellState.Sunk : CellState.PlacedShip;
            }
        }

        public void Clear()
        {
            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    Grid[new Point(x,y)] = CellState.None;
        }
    }
}
