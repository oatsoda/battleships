using Battleships.GameEngine.Random;
using Battleships.GameEngine.Strategy;
using Moq;
using System.Collections.Generic;
using System.Drawing;
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
            m_Random = FixedRandom("F5", "G7").Object;
            var prevShots = PrevShots("F5");
            var strategy = CreateStrategy();

            // When
            var r = strategy.NextTarget(new List<Point>(), prevShots);

            // Then
            Assert.Equal("G7", r);
        }

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
            var strategy = CreateStrategy();
            var prevShots = PrevShots("B0", "B2", "A1");

            // When
            var r = strategy.NextTarget(new List<Point> { new GridSquare("B1").Point }, prevShots);

            // Then
            Assert.Equal("C1", r);
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
    }
}
