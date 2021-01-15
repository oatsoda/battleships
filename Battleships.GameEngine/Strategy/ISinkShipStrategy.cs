using System.Collections.Generic;
using System.Drawing;

namespace Battleships.GameEngine.Strategy
{
    public interface ISinkShipStrategy
    {
        Point NextTarget(List<Point> unsunkShipHits, ShotState[,] previousShots);
    }
}
