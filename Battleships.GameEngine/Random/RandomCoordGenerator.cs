using System.Drawing;

namespace Battleships.GameEngine.Random
{
    public class RandomCoordGenerator : IRandomCoordGenerator
    {
        private static System.Random s_Random = new System.Random();

        public string GetRandomCoord()
        {
            var c = (char)s_Random.Next(65, 75);
            return $"{c}{s_Random.Next(0,10)}";
        }

        public (string start, string end) GetRandomShipCoords(int length)
        {
            var startX = s_Random.Next(0,10);
            var startY = s_Random.Next(0,10);
            var isVertical = s_Random.Next(0, 2) == 1;
            var positiveDir = s_Random.Next(0, 2) == 1;

            int endX;
            int endY;

            // Attempt to create end point
            if (isVertical)
            {
                endX = startX;
                (startY, endY) = Calculate1DStartEndForLength(startY, length, positiveDir);
            }
            else
            {
                endY = startY;
                (startX, endX) = Calculate1DStartEndForLength(startX, length, positiveDir);
            }

            return ($"{(char)(startX+65)}{startY}", $"{(char)(endX+65)}{endY}");
        }

        /// <summary>
        /// Given the start, will calculate the 1D end (and adjust start if required) to fit within the 0-9 range of grid squares.
        /// </summary>
        /// <param name="start">The initial start location (0-9)</param>
        /// <param name="length">The length required including start and end.</param>
        /// <returns>Start and End values (0-9)</returns>
        private (int start, int end) Calculate1DStartEndForLength(int start, int length, bool positiveDir)
        {
            var offset = length - 1; // Num spaces from start

            // Calculate how many free squares are after/before the start
            var spare = positiveDir ? (9 - start) : start;

            // If not enough, move start to allow
            if (spare < offset)
            {
                var adjust = offset - spare;
                start -= positiveDir ? adjust : -adjust;
            }

            // Finally add/subtract from start to get end
            return (start, start + (positiveDir ? offset : -offset));
        }

    }
}
