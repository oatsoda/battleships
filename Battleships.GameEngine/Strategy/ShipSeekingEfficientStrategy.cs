using Battleships.GameEngine.Random;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleships.GameEngine.Strategy
{
    public class ShipSeekingEfficientStrategy : IComputerStrategy
    {
        private IRandomCoordGenerator m_RandomCoordGenerator;

        public ShipSeekingEfficientStrategy() : this(new RandomCoordGenerator())
        {

        }

        public ShipSeekingEfficientStrategy(IRandomCoordGenerator randomCoordGenerator)
        {
            m_RandomCoordGenerator = randomCoordGenerator;
        }

        public GridSquare NextTarget(List<Point> unsunkShipHits, ShotState[,] previousShots) 
        {
            if (unsunkShipHits.Count == 0)
                return GetRandom(previousShots);

            var potentials = new List<Point>();
            foreach (var hit in unsunkShipHits)
            {
                potentials.AddRange(AdjacentPoints(hit));
            }

            potentials.RemoveAll(p => unsunkShipHits.Contains(p) || previousShots[p.X, p.Y] != ShotState.NoShot);
            return new GridSquare(potentials.First());
        }        

        private static IEnumerable<Point> AdjacentPoints(Point point)
        {
            if (point.X > 0)
                yield return new Point(point.X - 1, point.Y);

            if (point.X < 9)
                yield return new Point(point.X + 1, point.Y);
            
            if (point.Y > 0)
                yield return new Point(point.X, point.Y - 1);

            if (point.Y < 9)
                yield return new Point(point.X, point.Y + 1);
        }

        private GridSquare GetRandom(ShotState[,] previousShots)
        {
            GridSquare target;
            do
            {
                target = m_RandomCoordGenerator.GetRandomCoord();
            }
            while (previousShots[target.Point.X, target.Point.Y] != ShotState.NoShot);
            return target;
        }
    }
}
