using System;
using System.Diagnostics;
using System.Linq;

namespace Battleships.GameEngine
{
    public class Game
    {
        private SetupBoard m_SetupBoard;

        public Players Turn { get; }

        public Game(SetupBoard setupBoard) 
        { 
            if (!setupBoard.IsValid)
                throw new ArgumentException("Start Board is not valid.",  nameof(setupBoard));

            m_SetupBoard = setupBoard;
            Turn = Players.PlayerOne;
        }

        public FireResult Fire(GridSquare target)
        {
            var isHit = m_SetupBoard.OccupationPointsByShip.ContainsKey(target.Point);
            var hitShip = isHit ? m_SetupBoard.OccupationPointsByShip[target.Point] : null;
            return new FireResult(isHit);
        }
    }

    public class FireResult
    {
        public bool IsHit { get; }

        public FireResult(bool isHit)
        {
            IsHit = isHit;
        }
    }
}
