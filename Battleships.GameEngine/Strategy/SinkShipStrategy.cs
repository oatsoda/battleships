using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleships.GameEngine.Strategy
{
    public class SinkShipStrategy : ISinkShipStrategy
    {
        public Point NextTarget(List<Point> unsunkShipHits, ShotState[,] previousShots)
        {
            var potentials = new List<Point>();
            foreach (var hit in unsunkShipHits)
            {
                potentials.AddRange(AdjacentPoints(hit));
            }

            potentials.RemoveAll(p => unsunkShipHits.Contains(p) || previousShots[p.X, p.Y] != ShotState.NoShot);
            return potentials.First();
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
    }
}
