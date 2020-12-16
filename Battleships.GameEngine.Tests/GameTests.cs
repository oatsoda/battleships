using System;
using System.Drawing;
using Xunit;

namespace Battleships.GameEngine.Tests
{
    public class GameTests
    {
        [Fact]
        public void GameCtorThrowsIfSetupBoardInvalid()
        {
            // Given
            var setupBoard = new SetupBoard();
            Assert.False(setupBoard.IsValid);

            // When
            var ex = Record.Exception(() => new Game(setupBoard));

            // Then
            Assert.NotNull(ex);
            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Contains("is not valid.", argEx.Message);
        }

        [Fact]
        public void GameCtorDoesNotThrowIfSetupBoardValid()
        {
            // Given
            var setupBoard = TestData.CreateValidSetupBoard();

            // When
            var ex = Record.Exception(() => new Game(setupBoard));

            // Then
            Assert.Null(ex);            
        }

        // TODO: After setup, game Start, turns, boards (shots + hits) and outcome.
    }

    public static class TestData
    {
        public static SetupBoard CreateValidSetupBoard()
        {
            var setupBoard = new SetupBoard();
            setupBoard.AddShip(CreateValidShipOfLength(2));
            setupBoard.AddShip(CreateValidShipOfLength(3));
            setupBoard.AddShip(CreateValidShipOfLength(3));
            setupBoard.AddShip(CreateValidShipOfLength(4));
            setupBoard.AddShip(CreateValidShipOfLength(5));
            Assert.True(setupBoard.IsValid);
            return setupBoard;
        }

        public static Ship CreateValidShipOfLength(int length)
        {
            return new Ship(new Point(0, 0), new Point(0, length));
        }
    }


    public class SetupBoardTests
    {
        [Fact]
        public void SetupBoardDefaultsInvalid()
        {
            // When
            var setupBoard = new SetupBoard();

            // Then
            Assert.False(setupBoard.IsValid);
        }
                
        [Fact]
        public void SetupBoardNotValidIfNotAllShipsSet()
        {
            // Given
            var setupBoard = new SetupBoard();

            // When
            setupBoard.AddShip(TestData.CreateValidShipOfLength(2));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(4));
            
            // Then
            Assert.False(setupBoard.IsValid);
        }

        [Fact]
        public void SetupBoardValidIfAllShipsSet()
        {
            // Given
            var setupBoard = new SetupBoard();

            // When
            setupBoard.AddShip(TestData.CreateValidShipOfLength(2));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(4));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(5));
            
            // Then
            Assert.True(setupBoard.IsValid);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(4)]
        [InlineData(3)]
        [InlineData(2)]
        public void SetupBoardNotvalidIfTooManyShipsForLength(int length)
        {
            // Given            
            var setupBoard = new SetupBoard();
            setupBoard.AddShip(TestData.CreateValidShipOfLength(2));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(4));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(5));
            Assert.True(setupBoard.IsValid);

            var excessShip = new Ship(new Point(0,0), new Point(0, length));
            Assert.Equal(length, excessShip.Length);
            
            // When
            setupBoard.AddShip(excessShip);

            // Then
            Assert.False(setupBoard.IsValid);
        }
    }

    public class ShipTests
    {
        [Theory]
        [InlineData(0, 0, 2, 2)]
        [InlineData(0, 0, 1, 2)]
        public void ShipCtorThrowsIfDiagonal(int startX, int startY, int endX, int endY)
        {
            // Given
            var start = new Point(startX, startY);
            var end = new Point(endX, endY);
            
            // When
            var ex = Record.Exception(() => new Ship(start, end));
            
            // Then
            Assert.NotNull(ex);
            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Contains("diagonal", argEx.Message);
        }
                
        [Theory]
        [InlineData(0, 0, 0, 1)]
        [InlineData(0, 0, 0, 6)]
        public void ShipCtorThrowsIfLengthNotBetween2And5(int startX, int startY, int endX, int endY)
        {
            // Given
            var start = new Point(startX, startY);
            var end = new Point(endX, endY);
            
            // When
            var ex = Record.Exception(() => new Ship(start, end));
            
            // Then
            Assert.NotNull(ex);
            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Contains("length must be between", argEx.Message);
        }
        
        [Theory]
        [InlineData(0, 0, 0, 2, 2, true)]
        [InlineData(0, 0, 0, 3, 3, true)]
        [InlineData(0, 0, 0, 4, 4, true)]
        [InlineData(0, 0, 0, 5, 5, true)]
        [InlineData(0, 0, 2, 0, 2, false)]
        [InlineData(0, 0, 3, 0, 3, false)]
        [InlineData(0, 0, 4, 0, 4, false)]
        [InlineData(0, 0, 5, 0, 5, false)]
        public void ShipHasCorrectDetails(int startX, int startY, int endX, int endY, int expectedLength, bool expectedIsVertical)
        {
            // Given
            var start = new Point(startX, startY);
            var end = new Point(endX, endY);
            
            // When
            var ship = new Ship(start, end);
            
            // Then
            Assert.Equal(expectedLength, ship.Length);
            Assert.Equal(expectedIsVertical, ship.IsVertical);
        }

        // TODO: Ships must not overlap
        // TODO: Ships must not exceed grid (9x9 grid, so 0-8)
    }
}
