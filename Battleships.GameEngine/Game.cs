using System;
using System.Diagnostics;

namespace Battleships.GameEngine
{
    public class Game
    {
        private SetupBoard m_StartBoard;

        public Game(SetupBoard startBoard) 
        { 
            if (!startBoard.IsValid)
                throw new ArgumentException("Start Board is not valid.",  nameof(startBoard));

            m_StartBoard = startBoard;

        }
    }
}
