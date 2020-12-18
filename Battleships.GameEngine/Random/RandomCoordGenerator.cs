using System;

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
    }
}
