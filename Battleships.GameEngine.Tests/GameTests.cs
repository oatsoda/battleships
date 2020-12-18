using System;
using Xunit;

namespace Battleships.GameEngine.Tests
{
    public class GameTests
    {
        private readonly SetupBoard m_SetupBoard = new SetupBoard();
        private readonly Game m_Game;

        public GameTests()
        {
            m_SetupBoard.AddShip(("A0", "A1"));
            m_SetupBoard.AddShip(("B0", "B2"));
            m_SetupBoard.AddShip(("C0", "C2"));
            m_SetupBoard.AddShip(("D0", "D3"));
            m_SetupBoard.AddShip(("E0", "E4"));

            m_Game = new Game(m_SetupBoard);
        }

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
            // When
            var ex = Record.Exception(() => new Game(m_SetupBoard));

            // Then
            Assert.Null(ex);            
        }

        // TODO: After setup, game Start, turns, boards (shots + hits) and outcome.

        [Fact]
        public void GameDefaultsToPlayersTurn()
        {
            // When
            var game = new Game(m_SetupBoard);

            // Then
            Assert.Equal(Players.PlayerOne, game.Turn);
        }
                
        [Theory]
        [InlineData("A2", false)]
        [InlineData("A1", true)]
        public void FireReturnsWhetherHit(string coords, bool expectedHit)
        {
            // When
            var result = m_Game.Fire(coords);

            // Then
            Assert.Equal(expectedHit, result.IsHit);
        }
         
        [Theory]
        [InlineData("A2", false)]
        [InlineData("A1", true)]
        public void FireReturnsWhetherShipSunk(string coords, bool expectedHit)
        {
            // When
            var result = m_Game.Fire(coords);

            // Then
            Assert.Equal(expectedHit, result.IsHit);
        }
    }
}
