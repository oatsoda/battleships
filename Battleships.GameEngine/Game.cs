using System;

namespace Battleships.GameEngine
{
    public class Game
    {
        private readonly SetupBoard m_SetupBoard;
        private readonly ShotState[,] m_PlayerOneShots = new ShotState[10, 10];

        public Players Turn { get; private set; }

        public Game(SetupBoard setupBoard) 
        { 
            if (!setupBoard.IsValid)
                throw new ArgumentException("Start Board is not valid.",  nameof(setupBoard));

            m_SetupBoard = setupBoard;
            Turn = Players.PlayerOne;
        }

        public FireResult Fire(GridSquare target)
        {
            if (Turn != Players.PlayerOne)
                throw new InvalidOperationException("It is not your turn.");

            if (m_PlayerOneShots[target.Point.X, target.Point.Y] != ShotState.NoShot)
                throw new ArgumentException($"You have already fired on {target}.");

            Turn = Players.PlayerTwo;

            var isHit = m_SetupBoard.OccupationPointsByShip.ContainsKey(target.Point);

            m_PlayerOneShots[target.Point.X, target.Point.Y] = isHit ? ShotState.Hit : ShotState.Miss;

            if (!isHit)
                return new FireResult();

            var hitShip = m_SetupBoard.OccupationPointsByShip[target.Point];
            var isSunk = hitShip.Hit(target.Point);
            
            return new FireResult(isHit, isSunk, isSunk ? hitShip.Length : null);
        }

        public void OpponentsTurn()
        {
            if (Turn != Players.PlayerTwo)
                throw new InvalidOperationException("It is not your opponents turn. It is your turn.");

            Turn = Players.PlayerOne;
        }
    }

    public class FireResult
    {
        public bool IsHit { get; }
        public bool IsSunkShip { get; }
        public int? ShipSunkSize { get; }

        public FireResult()
        {
        }

        public FireResult(bool isHit, bool isSunkShip, int? sizeIfSunk)
        {
            IsHit = isHit;
            IsSunkShip = isSunkShip;
            ShipSunkSize = sizeIfSunk;
        }
    }

    public enum ShotState
    {
        NoShot,
        Miss,
        Hit
    }

}
