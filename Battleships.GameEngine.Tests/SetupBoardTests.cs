using System.Linq;
using Xunit;

namespace Battleships.GameEngine.Tests
{
    public class SetupBoardTests
    {
        private readonly SetupBoard m_SetupBoard = new SetupBoard();

        public SetupBoardTests()
        {
            m_SetupBoard.AddShip(("A0", "A1"));
            m_SetupBoard.AddShip(("B0", "B2"));
            m_SetupBoard.AddShip(("C0", "C2"));
            m_SetupBoard.AddShip(("D0", "D3"));
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
            Assert.False(m_SetupBoard.IsValid);
        }

        [Fact]
        public void SetupBoardValidIfAllShipsSet()
        {
            // When
            var result = m_SetupBoard.AddShip(("E0", "E4"));
            
            // Then
            Assert.True(result.Success);
            Assert.True(m_SetupBoard.IsValid);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(4)]
        [InlineData(3)]
        [InlineData(2)]
        public void AddShipFailsIfTooManyShipsForLength(int length)
        {
            // Given          
            m_SetupBoard.AddShip(("E0", "E4"));
            Assert.True(m_SetupBoard.IsValid);

            var excessShip = new Ship("F0", $"F{length-1}");
            Assert.Equal(length, excessShip.Length);
            
            // When
            var result = m_SetupBoard.AddShip(excessShip);

            // Then
            Assert.False(result.Success);
            Assert.Contains($"already enough ships of length {length}", result.Error);
        }

        [Fact]
        public void AddShipFailsIfIfShipsOverlap()
        {                        
            // When
            var result = m_SetupBoard.AddShip(("D3", "D7"));

            // Then
            Assert.False(result.Success);
            Assert.Contains("ship overlaps an existing ship", result.Error);
        }

        [Fact]
        public void SetupBoardHasCorrectNextShipRequired()
        {  
            var setupBoard = new SetupBoard();
            Assert.Equal(5, setupBoard.NextShip);
                        
            setupBoard.AddShip(("E0", "E4"));
            Assert.Equal(4, setupBoard.NextShip);

            setupBoard.AddShip(("D0", "D3"));
            Assert.Equal(3, setupBoard.NextShip);

            setupBoard.AddShip(("C0", "C2"));
            Assert.Equal(3, setupBoard.NextShip);

            setupBoard.AddShip(("B0", "B2"));
            Assert.Equal(2, setupBoard.NextShip);

            setupBoard.AddShip(("A0", "A1"));
            Assert.Null(setupBoard.NextShip);
        }

        [Fact]
        public void GenerateRandomCreatesValidRandomBoard()
        {                        
            // Given
            var setupBoardOne = new SetupBoard();
            var setupBoardTwo = new SetupBoard();
            
            // When
            setupBoardOne.GenerateRandom();
            setupBoardTwo.GenerateRandom();

            // Then
            Assert.True(setupBoardOne.IsValid);
            Assert.True(setupBoardTwo.IsValid);
            Assert.False(setupBoardOne.ShipsByOccupationPoints.Keys.All(p => setupBoardTwo.ShipsByOccupationPoints.ContainsKey(p)));
        }
    }
}
