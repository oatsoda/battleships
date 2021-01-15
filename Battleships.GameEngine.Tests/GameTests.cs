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

            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, new RandomCoordGenerator(), new SinkShipStrategy());
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
            var randomCoordsToGenerate = new[] { "I8", "I8", "J5" };

            // Given
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, RandomReturn(randomCoordsToGenerate).Object, new SinkShipStrategy());
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

        [Fact]
        public void OpponentsTurnFinishesOffShipIfFound()
        {
            // Given
            var setupBoard = new SetupBoard();
            setupBoard.AddShip(("A0", "A4"));
            setupBoard.AddShip(("A5", "A8"));
            setupBoard.AddShip(("B0", "B2"));
            setupBoard.AddShip(("B3", "B5"));
            setupBoard.AddShip(("G3", "G4"));
            
            m_Game = new Game(setupBoard, m_OpponentSetupBoard, RandomReturn("G3", "J7").Object, new SinkShipStrategy());
            
            m_Game.Fire("A0");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);
            var preResult = m_Game.OpponentsTurn();
            Assert.True(preResult.IsHit);

            FireResult PlayTurn(int turnNumber)
            {
                m_Game.Fire(GetSequentialCoord(turnNumber));
                return m_Game.OpponentsTurn();
            }

            // When
            const int expectedSunkWithinTurns = 8;
            int? sunkAfter = null;
            for (var x = 1; x <= expectedSunkWithinTurns;x++)
            {   
                if (PlayTurn(x).IsSunkShip)
                {
                    sunkAfter = x;
                    break;
                }
            }

            // Then
            Assert.NotNull(sunkAfter);
            Assert.True(sunkAfter <= expectedSunkWithinTurns);

            // And
            var nextResult = PlayTurn(sunkAfter.Value + 1);
            Assert.Equal("J7", nextResult.Target.ToString());
        }
        
        [Fact]
        public void OpponentsTurnFinishesOffAdjacentShipsIfFound()
        {
            // Given
            var setupBoard = new SetupBoard();
            setupBoard.AddShip(("A0", "A4"));
            setupBoard.AddShip(("A5", "A8"));
            setupBoard.AddShip(("B0", "B2"));
            setupBoard.AddShip(("G5", "G7"));
            setupBoard.AddShip(("G3", "G4"));

            var sinkShipStrategy = new Mock<ISinkShipStrategy>();
            sinkShipStrategy.Setup(s => s.NextTarget(It.IsAny<List<Point>>(), It.IsAny<ShotState[,]>()))
                            .Returns<List<Point>, ShotState[,]>((h,p) => {
                                
                                    // Force sink ship strategy to return a fixed result for the first call to ensure it does actually hit the adjacent ship (random cannot guarentee)
                                    if (h.Count == 1 && h[0] == new GridSquare("G4").Point)
                                        return new GridSquare("G5").Point;

                                    return new SinkShipStrategy().NextTarget(h,p);
                                }
                            );

            m_Game = new Game(setupBoard, m_OpponentSetupBoard, RandomReturn("G4", "J7").Object, sinkShipStrategy.Object);
            
            m_Game.Fire("A0");
            Assert.Equal(Players.PlayerTwo, m_Game.Turn);
            var preResult = m_Game.OpponentsTurn();
            Assert.True(preResult.IsHit);

            FireResult PlayTurn(int turnNumber)
            {
                m_Game.Fire(GetSequentialCoord(turnNumber));
                var r = m_Game.OpponentsTurn();
                m_TestOutputHelper.WriteLine($"Fired at {r.Target}. Hit: {r.IsHit} Sunk: {r.IsSunkShip}");
                return r;
            }

            // When
            const int expectedBothSunkWithinTurns = 17;
            var firstSunk = false;
            int? sunkAfter = null;
            for (var x = 1; x <= expectedBothSunkWithinTurns;x++)
            {   
                if (PlayTurn(x).IsSunkShip)
                {
                    if (!firstSunk)
                    {
                        firstSunk = true;
                        continue;
                    }

                    sunkAfter = x;
                    break;
                }
            }

            // Then
            Assert.NotNull(sunkAfter);
            Assert.True(sunkAfter <= expectedBothSunkWithinTurns);
        }
                
        [Theory]
        [InlineData("F6", false)]
        [InlineData("A0", true)]
        public void OpponentsTurnReturnsWhetherHit(string coords, bool expectedHit)
        {
            // Given
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, RandomReturn(coords).Object, new SinkShipStrategy());
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

        // TODO: Now that the computer has a strategy for finishing off ships, it won't use random for all, so it may take a little longer - need the test to keep trying until done
        [Theory(Skip = "todo")]
        [MemberData(nameof(OpponentSunkShots))]
        public void OpponentsTurnReturnsWhetherSunk(string[] coords, bool expectedSunk, int? expectedSunkSize)
        {
            // Given
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, RandomReturn(coords).Object, new SinkShipStrategy());
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


        // TODO: Now that the computer has a strategy for finishing off ships, it won't use random for all, so it may take a little longer - need the test to keep trying until done
        [Theory(Skip = "todo")]
        [MemberData(nameof(OpponentWonShots))]
        public void OpponentsTurnReturnsWhetherWon(string[] coords)
        {
            // Given
            m_Game = new Game(m_SetupBoard, m_OpponentSetupBoard, RandomReturn(coords).Object, new SinkShipStrategy());
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

        private static Mock<IRandomCoordGenerator> RandomReturn(params string[] coords)
        {
            var random = new Mock<IRandomCoordGenerator>();
            var r = random.SetupSequence(r => r.GetRandomCoord())
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
