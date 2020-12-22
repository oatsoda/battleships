using System;
using System.Linq;

namespace Battleships.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var p1 = GridDisplay.DrawGrid(10, 6);

            for (int i = 0; i < p1.GridSpaces.GetLength(0); i++)
                for (int j = 0; j < p1.GridSpaces.GetLength(1); j++)
                    i.ToString().DrawAt(p1.GridSpaces[i,j].x, p1.GridSpaces[i,j].y);

            GridDisplay.DrawGrid(p1.MaxX + 10, 6);

            Console.ReadLine();
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
#pragma warning disable IDE1006 // Naming Styles

        private const int MAX_ROWS = 10;
        private const int MAX_COLS = 10;

        private const int PER_CELL_X = 6;
        private const int PER_CELL_Y = 4;

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
            for (int i = 1; i <= MAX_ROWS; i++)
                DrawGridRow(i, x, y);

            var cellMidX = PER_CELL_X / 2;
            var cellMidY = PER_CELL_Y / 2;
            var g = Enumerable.Range(0, MAX_COLS)
                                    .Select(c => 
                                    {
                                        var cX = x+(c*PER_CELL_X)+cellMidX;
                                        return Enumerable.Range(0, MAX_ROWS)
                                                                 .Select(r => (cX, y+(r*PER_CELL_Y)+cellMidY)).ToArray();
                                                                 
                                    }).ToArray();

            var spaces = new (int x, int y)[MAX_COLS, MAX_ROWS];
            for (var c = 0; c < g.Count(); c++)
                for (var r = 0; r < g[c].Count(); r++)
                    spaces[c,r] = g[c][r];

            return new PlayerGrid(x+(PER_CELL_X*MAX_COLS)+1, y+(PER_CELL_Y*MAX_ROWS)+1, spaces);
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

        public static void DrawAt(int x, int y, string val)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(val);
        }
    }

    public static class StringExtensions
    {
        public static void DrawAt(this string val, int x, int y)
        {
            GridDisplay.DrawAt(x, y, val);
        }
    }
}
