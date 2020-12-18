using Xunit;

namespace Battleships.GameEngine.Tests
{
    public class GridSquareTests
    {
        [Theory]
        [InlineData("A0", 'A', 0, 0, 0)]
        [InlineData("A1", 'A', 1, 0, 1)]
        [InlineData("B0", 'B', 0, 1, 0)]
        [InlineData("b1", 'B', 1, 1, 1)]
        [InlineData("J0", 'J', 0, 9, 0)]
        [InlineData("J9", 'J', 9, 9, 9)]
        [InlineData("j8", 'J', 8, 9, 8)]
        [InlineData("F5", 'F', 5, 5, 5)]
        public void GridSquareHasCorrectProperties(string coord, char X, byte Y, int pointX, int pointY)
        {
            // When
            GridSquare gs = coord;

            // Then
            Assert.Equal(X, gs.X);
            Assert.Equal(Y, gs.Y);
            Assert.Equal(pointX, gs.Point.X);
            Assert.Equal(pointY, gs.Point.Y);
        }
    }
}
