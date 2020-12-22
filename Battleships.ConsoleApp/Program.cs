using System;
using System.Text;

namespace Battleships.ConsoleApp
{
    class Program
    {
        static void Main(string[] args)
        {
            var gridDisplay = new GridDisplay();
            gridDisplay.DrawGrid(10, 10);
            
            gridDisplay.DrawGrid(80, 10);

            Console.ReadLine();
        }
    }

    public class GridDisplay
    {
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

        public void DrawGrid(int x, int y)
        {
            for (int i = 1; i <= MAX_ROWS; i++)
                DrawGridRow(i, x, y);
        }

        private void DrawGridRow(int rowNumber, int startX, int startY)
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

        private void DrawCell(int startX, int startY, bool isOuterVert, bool isOuterHoriz)
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

        private static void DrawAt(int x, int y, string val)
        {
            Console.SetCursorPosition(x, y);
            Console.Write(val);
        }
    }
}
