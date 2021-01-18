using Battleships.GameEngine.Random;
using Battleships.GameEngine.Strategy;
using Moq;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using Xunit;

namespace Battleships.GameEngine.Tests.Strategy
{
    public class ShipSeekingEfficientStrategyTests
    {
        private IRandomCoordGenerator m_Random;

        public ShipSeekingEfficientStrategyTests()
        {
            m_Random = new RandomCoordGenerator();
        }

        [Fact]
        public void NextTargetReturnsRandomIfNoCurrentUnsunkHits()
        {
            // Given
            m_Random = FixedRandom("F5").Object;
            var strategy = CreateStrategy();

            // When
            var r = strategy.NextTarget(new List<Point>(), new ShotState[10,10]);

            // Then
            Assert.Equal("F5", r);
        }

        [Fact]
        public void NextTargetDoesNotReturnATargetAlreadyShotAt()
        {
            // Given
            var prevShots = PrevShots("F5");
            m_Random = FixedRandom("F5", "G7").Object;
            var strategy = CreateStrategy();

            // When
            var r = strategy.NextTarget(new List<Point>(), prevShots);

            // Then
            Assert.Equal("G7", r);
        }
        
        [Fact]
        public void NextTargetDoesNotReturnATargetWhichIsASingleSquare()
        {
            // Given
            var prevShots = PrevShots("B0", "A1", "B2", "C1");
            m_Random = FixedRandom("B1", "F8").Object;
            var strategy = CreateStrategy();

            // When
            var r = strategy.NextTarget(new List<Point>(), prevShots);

            // Then
            Assert.Equal("F8", r);
        }

        // TODO: Need to know what ships have been sunk as no point firing on gaps of X if ship is bigger.

        [Fact]
        public void NextTargetReturnsAdjacentSquareIfCurrentUnsunkHit()
        {
            // Given
            var strategy = CreateStrategy();

            // When
            var r = strategy.NextTarget(new List<Point> { new GridSquare("B1").Point }, new ShotState[10,10]);

            // Then
            Assert.Contains(r, new GridSquare[] { "A1", "B0", "B2", "C1"});
        }

        [Fact]
        public void NextTargetDoesNotReturnAnAdjacentSquareWhichHasAlreadyBeenShotAtIfCurrentUnsunkHit()
        {
            // Given
            var unsunkHits = UnsunkHits("B1");
            var prevShots = PrevShots("B0", "B2", "A1");
            var strategy = CreateStrategy();

            // When
            var r = strategy.NextTarget(unsunkHits, prevShots);

            // Then
            Assert.Equal("C1", r);
        }

        [Fact]
        public void NextTargetReturnsAnAdjacentSquareInSameAxisAsUnsunkHits()
        {
            // Given
            var unsunkHits = UnsunkHits("B1", "B2");
            var prevShots = PrevShots();
            var strategy = CreateStrategy();

            // When
            var r = strategy.NextTarget(unsunkHits, prevShots);

            // Then
            Assert.Contains(r, new GridSquare[] { "B0", "B3" });
        }

        private ShipSeekingEfficientStrategy CreateStrategy()
        {
            return new ShipSeekingEfficientStrategy(
                m_Random
                );
        }

        private static Mock<IRandomCoordGenerator> FixedRandom(params string[] coords)
        {
            var random = new Mock<IRandomCoordGenerator>();
            var r = random.SetupSequence(r => r.GetRandomCoord())
                          .Returns(coords[0]);

            for (int i = 1; i < coords.Length; i++)
                r = r.Returns(coords[i]);

            return random;
        }

        private static ShotState[,] PrevShots(params GridSquare[] targets)
        {
            var prevShots = new ShotState[10,10];
            foreach (var target in targets)
                prevShots[target.Point.X, target.Point.Y] = ShotState.Miss;
            return prevShots;
        }

        private static List<Point> UnsunkHits(params GridSquare[] hits)
        {
            return hits.Select(g => g.Point).ToList();
        }
    }
}
