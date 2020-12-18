using System;

namespace Battleships.GameEngine
{
    public class Game
    {
        private readonly SetupBoard m_PlayerOneSetup;
        private readonly ShotState[,] m_PlayerOneShots = new ShotState[10, 10];

        private readonly SetupBoard m_PlayerTwoSetup;        

        public Players Turn { get; private set; }

        public Game(SetupBoard setupBoard) 
        { 
            if (!setupBoard.IsValid)
                throw new ArgumentException("Setup Board is not valid.",  nameof(setupBoard));

            m_PlayerOneSetup = setupBoard;
            Turn = Players.PlayerOne;
        }

        internal Game(SetupBoard setupBoard, SetupBoard opponentsSetupBoard) : this(setupBoard)
        {
            if (!setupBoard.IsValid)
                throw new ArgumentException("Opponent Board is not valid.",  nameof(setupBoard));

            m_PlayerTwoSetup = opponentsSetupBoard;
        }


        public FireResult Fire(GridSquare target)
        {
            if (Turn != Players.PlayerOne)
                throw new InvalidOperationException("It is not your turn.");

            if (m_PlayerOneShots[target.Point.X, target.Point.Y] != ShotState.NoShot)
                throw new ArgumentException($"You have already fired on {target}.");

            Turn = Players.PlayerTwo;

            var isHit = m_PlayerTwoSetup.ShipsByOccupationPoints.ContainsKey(target.Point);

            m_PlayerOneShots[target.Point.X, target.Point.Y] = isHit ? ShotState.Hit : ShotState.Miss;

            if (!isHit)
                return new FireResult();

            var hitShip = m_PlayerTwoSetup.ShipsByOccupationPoints[target.Point];
            var isSunk = hitShip.Hit(target.Point);

            var haveWon = isSunk ? m_PlayerTwoSetup.AllSunk : false;
            
            return new FireResult(isHit, isSunk, isSunk ? hitShip.Length : null, haveWon);
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
        public bool HaveWon { get; set; }

        public FireResult()
        {
        }

        public FireResult(bool isHit, bool isSunkShip, int? sizeIfSunk, bool haveWon)
        {
            IsHit = isHit;
            IsSunkShip = isSunkShip;
            ShipSunkSize = sizeIfSunk;
            HaveWon = haveWon;
        }
    }

    public enum ShotState
    {
        NoShot,
        Miss,
        Hit
    }

}
