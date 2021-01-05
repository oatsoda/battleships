using System;
using System.Linq;

namespace Battleships.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var p1 = GridDisplay.DrawGrid(3, 2);
            var p2 = GridDisplay.DrawGrid(p1.MaxX + 10, 2);

            var commandInput = new CommandInput(0, p1.MaxY + 3);
            commandInput.WaitForInput("Enter coords of Aircraft Carrier (Length 5), e.g. A0 A4");
        }
    }

    public class SetupInput 
    {
        public void RunSetup()
        {

        }
    }

    public class CommandInput
    {
        private readonly int m_X;
        private readonly int m_MessageY;
        private readonly int m_InputY;

        public CommandInput(int x, int y)
        {
            m_X = x;
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

        private void ClearRow(int y)
        {
            for (int x = 0; x < Console.WindowWidth; x++)
                " ".DrawAt(x, y);            
        }
    }

    public class PlayerGrid
    {
        public int MaxX { get; private set; }
        public int MaxY { get; private set; }

        public (int x, int y)[,] GridSpaces { get; private set; }

        public PlayerGrid(int maxX, int maxY, (int x, int y)[,] gridSpaces)
        {
            MaxX = maxX;
            MaxY = maxY;
            GridSpaces = gridSpaces;
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

        private const int PER_CELL_X = 5;
        private const int PER_CELL_Y = 3;

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
