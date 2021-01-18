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
                potentials.AddRange(AdjacentPoints(hit));
            
            potentials.RemoveAll(p => unsunkShipHits.Contains(p) || previousShots[p.X, p.Y] != ShotState.NoShot);
            return PickBestPotential(potentials, unsunkShipHits, previousShots);
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
            while (previousShots[target.Point.X, target.Point.Y] != ShotState.NoShot ||
                    AllAdjacentHaveBeenShotAt(target.Point, previousShots)
                    );
            return target;
        }

        private static bool AllAdjacentHaveBeenShotAt(Point target, ShotState[,] previousShots)
        {
            foreach (var adj in AdjacentPoints(target))
                if (previousShots[adj.X, adj.Y] == ShotState.NoShot)
                    return false;            

            return true;
        }

        private static GridSquare PickBestPotential(List<Point> potentials, List<Point> unsunkShipHits, ShotState[,] previousShots)
        {
            Point bestPoint;
            if (potentials.Count > 1)
            {
                var x = unsunkShipHits.GroupBy(p => p.X).Select(g => new { g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).First();
                var y = unsunkShipHits.GroupBy(p => p.Y).Select(g => new { g.Key, Count = g.Count() }).OrderByDescending(g => g.Count).First();

                if (x.Count > y.Count)
                    bestPoint = potentials.OrderByDescending(p => p.X == x.Key).First();
                else
                    bestPoint = potentials.OrderByDescending(p => p.Y == y.Key).First();
            }
            else
            {
                bestPoint = potentials.First();
            }

            return new GridSquare(bestPoint);
        }
    }
}
