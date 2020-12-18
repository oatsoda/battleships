using System;
using System.Collections.Generic;
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

        [Fact]
        public void GameDefaultsToPlayersTurn()
        {
            // When
            var game = new Game(m_SetupBoard);

            // Then
            Assert.Equal(Players.PlayerOne, game.Turn);
        }
        
        [Fact]
        public void FireChangesTurnToOpponent()
        {
            // When
            m_Game.Fire("A0");

            // Then
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);
        }
        
        [Fact]
        public void FireThrowsIfNotPlayersTurn()
        {
            // Given
            m_Game.Fire("A0");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);

            // When
            var ex = Record.Exception(() => m_Game.Fire("A1"));

            // Then
            Assert.NotNull(ex);
            var argEx = Assert.IsType<InvalidOperationException>(ex);
            Assert.Contains("not your turn", argEx.Message);
        }

        [Fact]
        public void OpponentsTurnThrowsIfPlayersTurn()
        {
            // When
            var ex = Record.Exception(() => m_Game.OpponentsTurn());

            // Then
            Assert.NotNull(ex);
            var argEx = Assert.IsType<InvalidOperationException>(ex);
            Assert.Contains("not your opponents turn", argEx.Message);
        }
        
        [Fact]
        public void OpponentsTurnChangesTurnToPlayer()
        {
            // Given
            m_Game.Fire("A0");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);

            // When
            m_Game.OpponentsTurn();

            // Then
            Assert.Equal(Players.PlayerOne, m_Game.Turn);
        }
        
        [Fact]
        public void FireThrowsIfGridSquareAlreadyFired()
        {
            // Given
            m_Game.Fire("A0");
            m_Game.OpponentsTurn();

            // When
            var ex = Record.Exception(() => m_Game.Fire("A0"));

            // Then
            Assert.NotNull(ex);
            var argEx = Assert.IsType<ArgumentException>(ex);
            Assert.Contains("already fired on A0", argEx.Message);
        }

        // TODO: REDO AS FIRING ON OWN SHIPS!!
                
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

        public static IEnumerable<object[]> Shots()
        {
            yield return new object[] { new[] { "A1", "A0" }, true, 2 };
            yield return new object[] { new[] { "B0", "B1" }, false, null };
            yield return new object[] { new[] { "A0", "B0", "B1", "B2" }, true, 3 };
        }
         
        [Theory]
        [MemberData(nameof(Shots))]
        public void FireReturnsWhetherShipSunk(string[] coords, bool expectedSunk, int? expectedSunkSize)
        {
            var x = 0;
            foreach (var c in coords)
            {
                var result = m_Game.Fire(c);
                if (++x != coords.Length)
                {
                    Assert.False(result.IsSunkShip);
                    Assert.Null(result.ShipSunkSize);
                }
                else
                {
                    Assert.Equal(expectedSunk, result.IsSunkShip);
                    Assert.Equal(expectedSunkSize, result.ShipSunkSize);
                }
                m_Game.OpponentsTurn();
            }
        }
        
    }
}
