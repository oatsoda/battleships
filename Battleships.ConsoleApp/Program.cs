using Battleships.GameEngine;
using System;
using System.Drawing;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;

namespace Battleships.ConsoleApp
{
    class Program
    {
        private static int s_VertPadding = 1;
        private static int s_HorizPadding = 3;

        static void Main(string[] args)
        {
            var p1 = GridDisplay.DrawGrid(s_HorizPadding, s_VertPadding);
            var p2 = GridDisplay.DrawGrid(p1.MaxX + s_HorizPadding, s_VertPadding);

            var setupInput = new SetupInput(p1.MaxY + s_VertPadding);
            if (!setupInput.RunSetup(p1))
                return;

            var playInput = new PlayInput(p1.MaxY + s_VertPadding);
            playInput.RunPlay(setupInput.SetupBoard, p1, p2);

#if DEBUG
            Console.SetCursorPosition(0, Console.CursorTop + 10);
#endif
        }
    }

    public class PlayInput
    {
        private static Regex s_Regex = new Regex("^[A-Ja-j]{1}[0-9]{1}$");

        private readonly CommandInput m_CommandInput;

        public PlayInput(int y)
        {
            m_CommandInput = new CommandInput(y);
        }

        public void RunPlay(SetupBoard setupBoard, PlayerGrid playerGrid, PlayerGrid computerGrid)
        {
            var game = new Game(setupBoard);
            var error = string.Empty;

            while (true)
            {
                // TODO: Add sound!

                if (game.Turn == Players.PlayerOne)
                {
                    var errorDisplay = error.Length > 0 ? $" [{error}]" : "";
                    var input = m_CommandInput.WaitForInput($"Enter a Target coordinate, e.g. A0{errorDisplay}");

                    if (!s_Regex.IsMatch(input))
                    {
                        error = $"Input not recognised as a Target Coordinate '{input}'";
                        continue;
                    }

                    FireResult fireResult;
                    try
                    {
                        fireResult = game.Fire(input);
                    }
                    catch (ArgumentException ex)
                    {
                        error = ex.Message;
                        continue;
                    }

                    error = string.Empty;

                    computerGrid.DrawTarget(fireResult.Target.Point, fireResult.IsHit);
                    m_CommandInput.ShowMessage(fireResult.IsSunkShip ? $"HIT {fireResult.Target}. YOU'VE SUNK A SHIP OF LENGTH {fireResult.ShipSunkSize}!" : fireResult.IsHit ? $"HIT {fireResult.Target}" : "Missed.");
                    
                    if (fireResult.HaveWon)
                    {
                        m_CommandInput.ShowResult("YOU WIN!!!");
                        break;
                    }
                }
                else
                {
                    var fireResult = game.OpponentsTurn();
                    playerGrid.DrawTarget(fireResult.Target.Point, fireResult.IsHit);
                    m_CommandInput.ShowMessage(fireResult.IsSunkShip ? $"Opponent Hits {fireResult.Target}. Has Sunk your {fireResult.ShipSunkSize}!" : fireResult.IsHit ? $"Opponent Hits {fireResult.Target}" : "Opponent Missed.");
                    if (fireResult.HaveWon)
                    {
                        m_CommandInput.ShowResult("YOU LOSE!!!");
                        break;
                    }
                }
                    
                Thread.Sleep(1000);
            }
        }
    }

    public class SetupInput 
    {
        private static Regex s_Regex = new Regex("^[A-Ja-j]{1}[0-9]{1} [A-Ja-j]{1}[0-9]{1}$");

        private readonly CommandInput m_CommandInput;

        public SetupBoard SetupBoard { get; private set; }

        public SetupInput(int y)
        {
            m_CommandInput = new CommandInput(y);
        }

        public bool RunSetup(PlayerGrid playerGrid)
        {
            SetupBoard = new SetupBoard();
            var error = string.Empty;

            while (!SetupBoard.IsValid)
            {                
                var errorDisplay = error.Length > 0 ? $" [{error}]" : "";
                var input = m_CommandInput.WaitForInput($"Enter coords of Ship length {SetupBoard.NextShip}, e.g. A0 A4{errorDisplay}");
                // TODO: Note that you can enter in any order, so why ask?

                if (input.ToLowerInvariant() == "exit")
                    return false;

                if (!s_Regex.IsMatch(input))
                {
                    error = $"Input not recognised as Ship Coordinates '{input}'";
                    continue;
                }

                var coords = input.Split(' ');
                Ship ship;
                try
                {
                    ship = new Ship(coords[0], coords[1]);
                }
                catch (ArgumentException ex)
                {
                    error = ex.Message;
                    continue;
                }

                error = string.Empty;

                var result = SetupBoard.AddShip(ship);

                if (!result.Success)
                    error = result.Error;
                else 
                    playerGrid.DrawShip(ship);                        
            }

            return true;
        }
    }

    public class CommandInput
    {
        private readonly int m_MessageY;
        private readonly int m_InputY;

        public CommandInput(int y)
        {
            m_MessageY = y;
            m_InputY = y+1;
        }

        public string WaitForInput(string message)
        {
            ClearRow(m_MessageY);
            ClearRow(m_InputY);
            message.DrawAt(0, m_MessageY);
            ">".DrawAt(0, m_InputY);
            Console.SetCursorPosition(2, m_InputY);
            return Console.ReadLine();
        }

        public void ShowMessage(string message)
        {
            ClearRow(m_MessageY);
            ClearRow(m_InputY);
            message.DrawAt(0, m_MessageY);
        }
        
        public void ShowResult(string message)
        {
            ClearRow(m_MessageY);
            ClearRow(m_InputY);
            message.DrawAt(0, m_InputY);
        }

        private void ClearRow(int y)
        {
            for (int x = 0; x < Console.WindowWidth; x++)
                " ".DrawAt(x, y);            
        }
    }

    public class PlayerGrid
    {
        private const string _UNSUNK = "▒";
        private const string _SUNK = "🔴";
        private const string _HIT = "X";
        private const string _MISS = "o";
        
        private const ConsoleColor _SHIP_COLOUR = ConsoleColor.Blue;
        private const ConsoleColor _HIT_COLOUR = ConsoleColor.Red;
        private const ConsoleColor _MISS_COLOUR = ConsoleColor.DarkGray;

        public int MaxX { get; private set; }
        public int MaxY { get; private set; }

        public (int x, int y)[,] GridSpaces { get; private set; }

        public PlayerGrid(int maxX, int maxY, (int x, int y)[,] gridSpaces)
        {
            MaxX = maxX;
            MaxY = maxY;
            GridSpaces = gridSpaces;
        }

        public void DrawShip(Ship ship)
        {
            foreach (var s in ship.Occupies)
            {
                var (x, y) = GridSpaces[s.X, s.Y];
                _UNSUNK.DrawAt(x, y, _SHIP_COLOUR);
            }
        }

        public void DrawTarget(Point target, bool isHit)
        {
            var (x, y) = GridSpaces[target.X, target.Y];
            (isHit ? _HIT : _MISS).DrawAt(x, y, isHit ? _HIT_COLOUR : _MISS_COLOUR);
        }
    }

    public static class GridDisplay
    {
        public static ConsoleColor DefaultTextColour = Console.ForegroundColor;
        public static ConsoleColor DefaultBgColour = Console.BackgroundColor;

        static GridDisplay()
        {
            Console.OutputEncoding = System.Text.Encoding.UTF8;
        }        

#pragma warning disable IDE1006 // Naming Styles

        private const int MAX_ROWS = 10;
        private const int MAX_COLS = 10;

        private const int PER_CELL_X = 4;
        private const int PER_CELL_Y = 2;

        private static int CellMidX = PER_CELL_X / 2;
        private static int CellMidY = PER_CELL_Y / 2;

        private static string OuterTopLeft = "╔";
        private static string OuterTopRight = "╗";
        private static string OuterBottomRight = "╝";
        private static string OuterBottomLeft = "╚";

        private static string OuterHoriz = "═";
        private static string OuterVert = "║";

        private static string OuterTopTee = "╤";
        private static string OuterRightTee = "╢";
        private static string OuterBottomTee = "╧";
        private static string OuterLeftTee = "╟";

        private static string InnerHoriz = "─";
        private static string InnerVert = "│";

        private static string InnerCross = "┼";

#pragma warning restore IDE1006 // Naming Styles

        public static PlayerGrid DrawGrid(int x, int y)
        {
            var maxX = x + (PER_CELL_X*(MAX_COLS+1));
            var maxY = y + (PER_CELL_Y*(MAX_ROWS+1));

            // TODO: This is very rough fix for too small console. Need to redo to adjust per cell values to fit instead. And error if not enough space. Linux support etc.
            if (maxX > Console.WindowWidth)
                Console.WindowWidth = maxX + PER_CELL_X;
            
            if (maxX > Console.WindowHeight)
                Console.WindowHeight = maxY + PER_CELL_Y + 3;

            DrawHeaderRow(x, y);
            DrawHeaderCol(x, y);
            x += (PER_CELL_X-1);
            y += (PER_CELL_Y-1);

            for (int i = 1; i <= MAX_ROWS; i++)
                DrawGridRow(i, x, y);

            var g = Enumerable.Range(0, MAX_COLS)
                                    .Select(c => 
                                    {
                                        var cX = x+(c*PER_CELL_X)+CellMidX;
                                        return Enumerable.Range(0, MAX_ROWS)
                                                                 .Select(r => (cX, y+(r*PER_CELL_Y)+CellMidY)).ToArray();
                                                                 
                                    }).ToArray();

            var spaces = new (int x, int y)[MAX_COLS, MAX_ROWS];
            for (var c = 0; c < g.Count(); c++)
                for (var r = 0; r < g[c].Count(); r++)
                    spaces[c,r] = g[c][r];

            return new PlayerGrid(x+(PER_CELL_X*MAX_COLS)+1, y+(PER_CELL_Y*MAX_ROWS)+1, spaces);
        }

        private static void DrawHeaderRow(int startX, int startY)
        {
            for (int i = 1; i <= MAX_COLS+1; i++)
            {
                for (int j = 0; j < PER_CELL_Y-1; j++)
                    DrawAt(startX+((i*PER_CELL_X)-1), startY+j, InnerVert, ConsoleColor.DarkGray);

                if (i > 1)
                    DrawAt((startX-1)+((i*PER_CELL_X)-CellMidX), (startY-1)+CellMidY, (i-2).AsUpperChar(), ConsoleColor.DarkGray);
            }
        }
        
        private static void DrawHeaderCol(int startX, int startY)
        {
            for (int i = 1; i <= MAX_ROWS+1; i++)
            {
                for (int j = 0; j < PER_CELL_X-1; j++)
                    DrawAt(startX+j, startY+((i*PER_CELL_Y)-1), InnerHoriz, ConsoleColor.DarkGray);

                if (i > 1)
                    DrawAt((startX-1)+CellMidX, (startY-1)+((i*PER_CELL_Y)-CellMidY), (i-2).ToString(), ConsoleColor.DarkGray);
            }
        }

        private static void DrawGridRow(int rowNumber, int startX, int startY)
        {
            var rowIndex = startY+((rowNumber-1)*PER_CELL_Y);

            // Draw Top Left
            var topLeft = rowNumber == 1 
                ? OuterTopLeft 
                : OuterLeftTee;
            DrawAt(startX, rowIndex, topLeft);

            // Draw Left-hand side            
            var maxOffsetIndex = PER_CELL_Y-1;
            for (int i = 1; i <= maxOffsetIndex; i++)
                DrawAt(startX, rowIndex+i, OuterVert);

            if (rowNumber == MAX_ROWS)
                DrawAt(startX, rowIndex+PER_CELL_Y, OuterBottomLeft);

            if (rowNumber == 1) // First row, draw top border
            {
                for (int i = 1; i < (MAX_COLS * PER_CELL_X); i++)
                    DrawAt(startX+i, rowIndex, i % PER_CELL_X == 0 ? OuterTopTee : OuterHoriz);
                DrawAt(startX+(MAX_COLS * PER_CELL_X), rowIndex, OuterTopRight);
            }

            for (int i = 0; i < MAX_COLS; i++)
                DrawCell(startX+(i * PER_CELL_X)+1, rowIndex+1, (i+1 == MAX_COLS), rowNumber == MAX_ROWS);            
        }

        private static void DrawCell(int startX, int startY, bool isOuterVert, bool isOuterHoriz)
        {            
            var maxOffsetIndexX = PER_CELL_X-1;
            var maxOffsetIndexY = PER_CELL_Y-1;

            // Draw right-hand side of cell
            for (int i = 0; i < maxOffsetIndexY; i++)
                DrawAt(startX+maxOffsetIndexX, startY+i, isOuterVert ? OuterVert : InnerVert);            

            // Draw bottom of cell
            for (int i = 0; i < maxOffsetIndexX; i++)
                DrawAt(startX+i, startY+maxOffsetIndexY, isOuterHoriz ? OuterHoriz : InnerHoriz);
            
            // Draw Bottom Right             
            var bottomRight = isOuterVert && isOuterHoriz
                ? OuterBottomRight
                : isOuterVert
                    ? OuterRightTee
                    : isOuterHoriz
                        ? OuterBottomTee
                        : InnerCross;

            DrawAt(startX+maxOffsetIndexX, startY+maxOffsetIndexY, bottomRight);
        }

        public static void DrawAt(int x, int y, string val, ConsoleColor? text = null, ConsoleColor? bg = null)
        {  
            Console.ForegroundColor = text ?? DefaultTextColour;
            Console.BackgroundColor = bg ?? DefaultBgColour;

            Console.SetCursorPosition(x, y);
            Console.Write(val);
        }
    }

    public static class StringExtensions
    {
        public static void DrawAt(this string val, int x, int y, ConsoleColor? text = null, ConsoleColor? bg = null)
        {
            GridDisplay.DrawAt(x, y, val, text, bg);
        }
    }

    public static class IntExtensions
    {
        public static string AsUpperChar(this int val) => ((char)(val+65)).ToString();
    }
}
