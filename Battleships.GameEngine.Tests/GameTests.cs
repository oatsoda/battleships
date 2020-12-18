using System;
using Xunit;
using Xunit.Abstractions;

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
            setupBoard.AddShip(("A0", "A1"));
            setupBoard.AddShip(("B0", "B2"));
            setupBoard.AddShip(("C0", "C2"));
            setupBoard.AddShip(("D0", "D3"));
            setupBoard.AddShip(("E0", "E4"));
            Assert.True(setupBoard.IsValid);
            return setupBoard;
        }

        public static Ship CreateValidShipOfLength(int length, int putOnCol = 0)
            {return ($"{(char)(65+putOnCol)}0", $"{(char)(65+putOnCol)}{length-1}");
        }
    }


    public class SetupBoardTests
    {
        private readonly ITestOutputHelper m_Output;

        public SetupBoardTests(ITestOutputHelper output)
        {
            m_Output = output;
        }

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
            setupBoard.AddShip(TestData.CreateValidShipOfLength(2, 0));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3, 1));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3, 2));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(4, 3));
            
            // Then
            Assert.False(setupBoard.IsValid);
        }

        [Fact]
        public void SetupBoardValidIfAllShipsSet()
        {
            // Given
            var setupBoard = new SetupBoard();

            // When
            setupBoard.AddShip(TestData.CreateValidShipOfLength(2, 0));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3, 1));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(3, 2));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(4, 3));
            setupBoard.AddShip(TestData.CreateValidShipOfLength(5, 4));
            
            // Then
            Assert.True(setupBoard.IsValid);

            setupBoard.OccupationPoints.ForEach(p => m_Output.WriteLine(p.ToString()));

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
            setupBoard.AddShip(("A0", "A1"));
            setupBoard.AddShip(("B0", "B2"));
            setupBoard.AddShip(("C0", "C2"));
            setupBoard.AddShip(("D0", "D3"));
            setupBoard.AddShip(("E0", "E4"));
            Assert.True(setupBoard.IsValid);

            var excessShip = new Ship("F0", $"F{length-1}");
            Assert.Equal(length, excessShip.Length);
            
            // When
            setupBoard.AddShip(excessShip);

            // Then
            Assert.False(setupBoard.IsValid);
        }

        [Fact]
        public void SetupBoardNotValidIfShipsOverlap()
        {
            // Given            
            var setupBoard = new SetupBoard();
            setupBoard.AddShip(("A0", "A1"));
            setupBoard.AddShip(("B0", "B2"));
            setupBoard.AddShip(("C0", "C2"));
            setupBoard.AddShip(("J0", "J3"));
                        
            // When
            setupBoard.AddShip(("J3", "J7"));

            // Then
            Assert.False(setupBoard.IsValid);
        }
    }

    public class ShipTests
    {
        [Theory]
        [InlineData("A0", "B1")]
        [InlineData("A0", "B2")]
        public void ShipCtorThrowsIfDiagonal(string start, string end)
        {
            // When
            var ex = Record.Exception(() => new Ship(start, end));
            
            // Then
            Assert.NotNull(ex);
            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Contains("diagonal", argEx.Message);
        }
                
        [Theory]
        [InlineData("A0", "A0")]
        [InlineData("A0", "A5")]
        public void ShipCtorThrowsIfLengthNotBetween2And5(string start, string end)
        {            
            // When
            var ex = Record.Exception(() => new Ship(start, end));
            
            // Then
            Assert.NotNull(ex);
            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Contains("length must be between", argEx.Message);
        }
        
        [Theory]
        [InlineData("A0", "A1", 2, true)]
        [InlineData("A0", "A2", 3, true)]
        [InlineData("A0", "A3", 4, true)]
        [InlineData("A0", "A4", 5, true)]
        [InlineData("J0", "J1", 2, true)]
        [InlineData("A0", "B0", 2, false)]
        [InlineData("A0", "C0", 3, false)]
        [InlineData("A0", "D0", 4, false)]
        [InlineData("A0", "E0", 5, false)]
        [InlineData("I0", "J0", 2, false)]
        [InlineData("B5", "B9", 5, true)]
        [InlineData("B9", "B5", 5, true)]
        public void ShipHasCorrectDetails(string start, string end, int expectedLength, bool expectedIsVertical)
        {            
            // When
            var ship = new Ship(start, end);
            
            // Then
            Assert.Equal(expectedLength, ship.Length);
            Assert.Equal(expectedIsVertical, ship.IsVertical);
        }
    }

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
