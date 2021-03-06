using Battleships.GameEngine.Random;
using Battleships.GameEngine.Strategy;
using Moq;
using System;
using System.Collections.Generic;
using System.Drawing;
using Xunit;
using Xunit.Abstractions;

namespace Battleships.GameEngine.Tests
{
    public class GameTests
    {
        private readonly SetupBoard m_SetupBoard = new SetupBoard();
        private readonly SetupBoard m_OpponentSetupBoard = new SetupBoard();
        private readonly ITestOutputHelper m_TestOutputHelper;
        private Game m_Game;

        public GameTests(ITestOutputHelper testOutputHelper)
        {
            m_SetupBoard.AddShip(("A0", "A1"));
            m_SetupBoard.AddShip(("B0", "B2"));
            m_SetupBoard.AddShip(("C0", "C2"));
            m_SetupBoard.AddShip(("D0", "D3"));
            m_SetupBoard.AddShip(("E0", "E4"));
            
            m_OpponentSetupBoard.AddShip(("F5", "F6"));
            m_OpponentSetupBoard.AddShip(("G5", "G7"));
            m_OpponentSetupBoard.AddShip(("H5", "H7"));
            m_OpponentSetupBoard.AddShip(("I5", "I8"));
            m_OpponentSetupBoard.AddShip(("J5", "J9"));

            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, new ShipSeekingEfficientStrategy());
            m_TestOutputHelper = testOutputHelper;
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

        [Theory]
        [InlineData("B5", false)]
        [InlineData("H6", true)]
        public void FireReturnsWhetherHit(string coords, bool expectedHit)
        {
            // When
            var result = m_Game.Fire(coords);

            // Then
            Assert.Equal(expectedHit, result.IsHit);
        }

        public static IEnumerable<object[]> SunkShots()
        {
            yield return new object[] { new[] { "F5", "F6" }, true, 2 };
            yield return new object[] { new[] { "F6", "F5" }, true, 2 };
            yield return new object[] { new[] { "F5", "G5" }, false, null };
            yield return new object[] { new[] { "F5", "F4" }, false, null };
            yield return new object[] { new[] { "A0", "I5", "I8", "I7", "I6" }, true, 4 };
        }

        [Theory]
        [MemberData(nameof(SunkShots))]
        public void FireReturnsWhetherSunk(string[] coords, bool expectedSunk, int? expectedSunkSize)
        {
            var x = 0;
            foreach (var c in coords)
            {
                var result = m_Game.Fire(c);
                if (++x != coords.Length)
                {
                    Assert.False(result.IsSunkShip);
                    Assert.Null(result.ShipSunk);
                }
                else
                {
                    Assert.Equal(expectedSunk, result.IsSunkShip);
                    if (!expectedSunk)
                    {
                        Assert.False(result.IsSunkShip);
                        Assert.Null(result.ShipSunk);
                    }
                    else
                    {
                        Assert.NotNull(result.ShipSunk);
                        Assert.Equal(expectedSunkSize, result.ShipSunk.Length);
                    }
                }
                m_Game.OpponentsTurn();
            }
        }

        public static IEnumerable<object[]> WonShots()
        {
            yield return new object[] { new[] { "F5", "F6", 
                                                "G5", "G6", "G7",
                                                "H5", "H6", "H7",
                                                "I5", "I6", "I7", "I8",
                                                "J5", "J6", "J7", "J8", "J9"
                                              } };
        }

        [Theory]
        [MemberData(nameof(WonShots))]
        public void FireReturnsWhetherWon(string[] coords)
        {
            var x = 0;
            foreach (var c in coords)
            {
                var result = m_Game.Fire(c);
                if (++x != coords.Length)
                {
                    Assert.False(result.HaveWon);
                }
                else
                {
                    Assert.True(result.HaveWon);
                }
                m_Game.OpponentsTurn();
            }
        }
                
        [Fact]
        public void OpponentsTurnReturnsGridSquare()
        {
            // Given
            m_Game.Fire("A0");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);

            // When
            var result = m_Game.OpponentsTurn();

            // Then
            Assert.NotEqual(default, result.Target);
            Assert.True(result.Target.X >= 'A' && result.Target.X <= 'J');
            Assert.True(result.Target.Y >= 0 && result.Target.Y <= 9);
        }
        
        [Fact]
        public void OpponentsTurnDoesNotFireAtSameGridSquareMoreThanOnce() 
        {
            var coordsToTargetStrategy = new[] { "I8", "I8", "J5" };

            // Given
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, FixedTargetStrategy(coordsToTargetStrategy).Object);
            m_Game.Fire("B4");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);
            var preResult = m_Game.OpponentsTurn();
            Assert.Equal("I8", preResult.Target.ToString());
            Assert.False(preResult.IsHit);
            m_Game.Fire("B5");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);

            // When
            var result = m_Game.OpponentsTurn();
            
            // Then
            Assert.Equal("J5", result.Target.ToString());
        } 

        /*
         Would have prefered to have behavioural tests for ensuring that ships hit are then hunted and sunk, but the random
         element makes that difficult, so it's now tested separately in the strategy.
         */

        [Fact]
        public void OpponentsTurnPassesPreviousShotsToStrategy()
        {
            var coordsToTargetStrategy = new[] { "I8", "J5" };

            // Given
            var computerStrategy = FixedTargetStrategy(coordsToTargetStrategy);
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, computerStrategy.Object);
            m_Game.Fire("B4");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);

            // When
            var firstResult = m_Game.OpponentsTurn();
            Assert.False(firstResult.IsHit);

            // Then
            computerStrategy.Verify(s => s.NextTarget(It.IsAny<List<Point>>(), It.Is<ShotState[,]>(st => st[firstResult.Target.Point.X, firstResult.Target.Point.Y] == ShotState.Miss)), Times.Once);

            // And Given
            m_Game.Fire("B5");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);

            // And When
            var secondResult = m_Game.OpponentsTurn();
            
            // Then
            Assert.False(secondResult.IsHit);
            computerStrategy.Verify(s => s.NextTarget(It.IsAny<List<Point>>(), It.Is<ShotState[,]>(st => st[secondResult.Target.Point.X, secondResult.Target.Point.Y] == ShotState.Miss)), Times.Exactly(2)); // <!-- Moq not tracking ref param values, so param is evaluted AFTER.
        }
        
        [Fact]
        public void OpponentsTurnPassesUnsunkHitsToStrategy()
        {
            var coordsToTargetStrategy = new[] { "C1", "I8" };

            // Given
            var computerStrategy = FixedTargetStrategy(coordsToTargetStrategy);
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, computerStrategy.Object);
            m_Game.Fire("B4");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);
            var firstResult = m_Game.OpponentsTurn();
            Assert.True(firstResult.IsHit);
            m_Game.Fire("B5");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);

            // When
            var secondResult = m_Game.OpponentsTurn();

            // Then
            computerStrategy.Verify(s => s.NextTarget(It.Is<List<Point>>(pts => pts.Contains(new GridSquare("C1").Point)), It.IsAny<ShotState[,]>()));
        }

                        
        [Theory]
        [InlineData("F6", false)]
        [InlineData("A0", true)]
        public void OpponentsTurnReturnsWhetherHit(string coords, bool expectedHit)
        {
            // Given
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, FixedTargetStrategy(coords).Object);
            m_Game.Fire("A0");

            // When
            var result = m_Game.OpponentsTurn();

            // Then
            Assert.Equal(expectedHit, result.IsHit);
        }

        public static IEnumerable<object[]> OpponentSunkShots()
        {
            yield return new object[] { new[] { "A0", "A1" }, true, 2 };
            yield return new object[] { new[] { "A1", "A0" }, true, 2 };
            yield return new object[] { new[] { "A0", "B0" }, false, null };
            yield return new object[] { new[] { "A1", "A2" }, false, null };
            yield return new object[] { new[] { "J9", "D3", "D0", "D1", "D2" }, true, 4 };
        }

        [Theory]
        [MemberData(nameof(OpponentSunkShots))]
        public void OpponentsTurnReturnsWhetherSunk(string[] coords, bool expectedSunk, int? expectedSunkSize)
        {
            // Given
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, FixedTargetStrategy(coords).Object);
            m_Game.Fire("A0");

            var x = 0;
            foreach (var _ in coords)
            {
                var result = m_Game.OpponentsTurn();
                if (++x != coords.Length)
                {
                    Assert.False(result.IsSunkShip);
                    Assert.Null(result.ShipSunk);
                }
                else
                {
                    Assert.Equal(expectedSunk, result.IsSunkShip);
                    if (!expectedSunk)
                    {
                        Assert.False(result.IsSunkShip);
                        Assert.Null(result.ShipSunk);
                    }
                    else
                    {
                        Assert.NotNull(result.ShipSunk);
                        Assert.Equal(expectedSunkSize, result.ShipSunk.Length);
                    }
                }
                m_Game.Fire(GetSequentialCoord(x));
            }
        }

        public static IEnumerable<object[]> OpponentWonShots()
        {
            yield return new object[] { new[] { "A0", "A1", 
                                                "B0", "B1", "B2",
                                                "C0", "C1", "C2",
                                                "D0", "D1", "D2", "D3",
                                                "E0", "E1", "E2", "E3", "E4"
                                              } };
        }

        [Theory]
        [MemberData(nameof(OpponentWonShots))]
        public void OpponentsTurnReturnsWhetherWon(string[] coords)
        {
            // Given
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, FixedTargetStrategy(coords).Object);
            m_Game.Fire("A0");

            var x = 0;
            foreach (var _ in coords)
            {
                var result = m_Game.OpponentsTurn();
                if (++x != coords.Length)
                {
                    Assert.False(result.HaveWon);
                }
                else
                {
                    Assert.True(result.HaveWon);
                }
                m_Game.Fire(GetSequentialCoord(x));
            }
        }

        private static Mock<IComputerStrategy> FixedTargetStrategy(params string[] coords)
        {
            var random = new Mock<IComputerStrategy>();
            var r = random.SetupSequence(r => r.NextTarget(It.IsAny<List<Point>>(), It.IsAny<ShotState[,]>()))
                          .Returns(coords[0]);

            for (int i = 1; i < coords.Length; i++)
                r = r.Returns(coords[i]);

            return random;
        }

        private static string GetSequentialCoord(int coordNumber)
        {
            var c = coordNumber / 10;
            var r = coordNumber % 10;
            return $"{(char)(c+65)}{r}";
        }
    }
}
