using Battleships.GameEngine.Random;
using System;

namespace Battleships.GameEngine
{
    public class Game
    {
        // TODO: Expose shot grids? Readonly 2d arrays?
        private readonly SetupBoard m_PlayerOneSetup;
        private readonly ShotState[,] m_PlayerOneShots = new ShotState[10, 10];

        private readonly SetupBoard m_PlayerTwoSetup;
        private readonly ShotState[,] m_PlayerTwoShots = new ShotState[10, 10];
        private readonly IRandomCoordGenerator m_RandomCoordGenerator;

        public Players Turn { get; private set; }
        
        public Game(SetupBoard setupBoard) : this(setupBoard, new SetupBoard().GenerateRandom())
        {
        }

        private Game(SetupBoard playerOneSetup, SetupBoard playerTwoSetup) 
        { 
            if (!playerOneSetup.IsValid)
                throw new ArgumentException("Player One setup board is not valid.",  nameof(playerOneSetup));
            
            if (!playerTwoSetup.IsValid)
                throw new ArgumentException("Player Two setup board is not valid.",  nameof(playerTwoSetup));

            m_PlayerOneSetup = playerOneSetup;
            m_PlayerTwoSetup = playerTwoSetup;

            Turn = Players.PlayerOne;
        }

        internal Game(SetupBoard setupBoard, SetupBoard opponentsSetupBoard, IRandomCoordGenerator randomCoordGenerator) : this(setupBoard, opponentsSetupBoard)
        {
            m_RandomCoordGenerator = randomCoordGenerator;
        }

        public FireResult Fire(GridSquare target)
        {
            if (Turn != Players.PlayerOne)
                throw new InvalidOperationException("It is not your turn.");

            if (m_PlayerOneShots[target.Point.X, target.Point.Y] != ShotState.NoShot)
                throw new ArgumentException($"You have already fired on {target}.");

            Turn = Players.PlayerTwo;
            
            return GetResult(target, m_PlayerTwoSetup, m_PlayerOneShots);
        }

        public FireResult OpponentsTurn()
        {
            if (Turn != Players.PlayerTwo)
                throw new InvalidOperationException("It is not your opponents turn. It is your turn.");

            Turn = Players.PlayerOne;

            var target = (GridSquare)m_RandomCoordGenerator.GetRandomCoord();

            return GetResult(target, m_PlayerOneSetup, m_PlayerTwoShots);
        }

        private static FireResult GetResult(GridSquare target, SetupBoard targetBoard, ShotState[,] shotRecorder)
        {
            var isHit = targetBoard.ShipsByOccupationPoints.ContainsKey(target.Point);
            
            shotRecorder[target.Point.X, target.Point.Y] = isHit ? ShotState.Hit : ShotState.Miss;

            if (!isHit)
                return new FireResult(target);            

            var hitShip = targetBoard.ShipsByOccupationPoints[target.Point];
            var isSunk = hitShip.Hit(target.Point);
            
            var haveWon = isSunk ? targetBoard.AllSunk : false;

            return new FireResult(target, isSunk, isSunk ? hitShip.Length : null, haveWon);
        }
    }

    public class FireResult
    {
        public GridSquare Target { get; }
        public bool IsHit { get; }
        public bool IsSunkShip { get; }
        public int? ShipSunkSize { get; }
        public bool HaveWon { get; set; }

        public FireResult(GridSquare targetMissed)
        {
            Target = targetMissed;
        }

        public FireResult(GridSquare targetHit, bool isSunkShip, int? sizeIfSunk, bool haveWon)
        {
            Target = targetHit;
            IsHit = true;
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
