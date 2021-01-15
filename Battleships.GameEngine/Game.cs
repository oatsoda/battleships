using Battleships.GameEngine.Random;
using Battleships.GameEngine.Strategy;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;

namespace Battleships.GameEngine
{
    public class Game
    {
        private readonly SetupBoard m_PlayerOneSetup;
        private readonly ShotState[,] m_PlayerOneShots = new ShotState[10, 10];

        private readonly SetupBoard m_PlayerTwoSetup;
        private readonly ShotState[,] m_PlayerTwoShots = new ShotState[10, 10];

        private readonly IRandomCoordGenerator m_RandomCoordGenerator;
        private readonly ISinkShipStrategy m_SinkShipStrategy;

        public Players Turn { get; private set; }

        private List<Point> m_PlayerTwoUnsunkHits = new List<Point>();
        
        public Game(SetupBoard setupBoard) : this(setupBoard, new SetupBoard().GenerateRandom())
        {
            m_RandomCoordGenerator = new RandomCoordGenerator();
            m_SinkShipStrategy = new SinkShipStrategy();
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

        internal Game(SetupBoard setupBoard, SetupBoard opponentsSetupBoard, IRandomCoordGenerator randomCoordGenerator, ISinkShipStrategy sinkShipStrategy) : this(setupBoard, opponentsSetupBoard)
        {
            m_RandomCoordGenerator = randomCoordGenerator;
            m_SinkShipStrategy = sinkShipStrategy;
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

            GridSquare target;
            do {
                if (m_PlayerTwoUnsunkHits.Count == 0)
                    target = m_RandomCoordGenerator.GetRandomCoord();
                else 
                    target = CalculateNextKnownShipTarget(); // If have hit but not yet sunk, zero in on same area
            } 
            while (m_PlayerTwoShots[target.Point.X, target.Point.Y] != ShotState.NoShot);

            var result = GetResult(target, m_PlayerOneSetup, m_PlayerTwoShots);

            if (result.IsSunkShip)
                m_PlayerTwoUnsunkHits.RemoveAll(p => result.ShipSunk.Occupies.Contains(p));
            else if (result.IsHit)
                m_PlayerTwoUnsunkHits.Add(result.Target.Point);

            return result;
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

            return new FireResult(target, isSunk, isSunk ? hitShip : null, haveWon);
        }

        private GridSquare CalculateNextKnownShipTarget()
        {
            if (m_PlayerTwoUnsunkHits.Count == 0)
                throw new InvalidOperationException("Can only calculate known ship targets if already hit but ship not sunk.");

            var target = m_SinkShipStrategy.NextTarget(m_PlayerTwoUnsunkHits, m_PlayerTwoShots);
            return new GridSquare(target);
        }
    }

    public class FireResult
    {
        public GridSquare Target { get; }
        public bool IsHit { get; }
        public bool IsSunkShip { get; }
        public Ship ShipSunk { get; }
        public bool HaveWon { get; set; }

        public FireResult(GridSquare targetMissed)
        {
            Target = targetMissed;
        }

        public FireResult(GridSquare targetHit, bool isSunkShip, Ship shipIfSunk, bool haveWon)
        {
            Target = targetHit;
            IsHit = true;
            IsSunkShip = isSunkShip;
            ShipSunk = shipIfSunk;
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
