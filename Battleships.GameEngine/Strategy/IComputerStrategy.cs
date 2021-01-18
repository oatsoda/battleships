using System.Collections.Generic;
using System.Drawing;

namespace Battleships.GameEngine.Strategy
{
    public interface IComputerStrategy
    {
        GridSquare NextTarget(List<Point> unsunkShipHits, ShotState[,] previousShots);
    }
}
