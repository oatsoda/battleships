using System;
using Xunit;

namespace Battleships.GameEngine.Tests
{
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
}
