using Xunit;
using Xunit.Abstractions;

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
            m_SetupBoard.AddShip(("E0", "E4"));
            
            // Then
            Assert.True(m_SetupBoard.IsValid);
        }

        [Theory]
        [InlineData(5)]
        [InlineData(4)]
        [InlineData(3)]
        [InlineData(2)]
        public void SetupBoardNotvalidIfTooManyShipsForLength(int length)
        {
            // Given          
            m_SetupBoard.AddShip(("E0", "E4"));
            Assert.True(m_SetupBoard.IsValid);

            var excessShip = new Ship("F0", $"F{length-1}");
            Assert.Equal(length, excessShip.Length);
            
            // When
            m_SetupBoard.AddShip(excessShip);

            // Then
            Assert.False(m_SetupBoard.IsValid);
        }

        [Fact]
        public void SetupBoardNotValidIfShipsOverlap()
        {                        
            // When
            m_SetupBoard.AddShip(("D3", "D7"));

            // Then
            Assert.False(m_SetupBoard.IsValid);
        }
    }
}
