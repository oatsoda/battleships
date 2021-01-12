using Battleships.GameEngine;
using System.Collections.Generic;
using System.Drawing;

namespace Battleships.ProgWebAppBlazor
{
    public enum CellState
    {
        None,
        PlacedShip,
        Miss,
        Hit,
        Sunk            
    }

    public class GridState
    {
        public Dictionary<Point, CellState> Grid { get; } 

        public GridState()
        {
            Grid = new Dictionary<Point, CellState>();

            for (int x = 0; x < 10; x++)
                for (int y = 0; y < 10; y++)
                    Grid[new Point(x,y)] = CellState.None;            
        }

        public void DrawShip(Ship ship)
        {
            foreach (var s in ship.Occupies)
            {
                Grid[s] = CellState.PlacedShip;
            }
        }
    }
}
