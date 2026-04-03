using System;
using System.Windows.Media.Imaging;

namespace Minesweeper
{
    public static class MinesweeperTextures
    {
        private static BitmapImage Load(string path) =>
            new BitmapImage(new Uri($"pack://application:,,,/{path}"));

        // === КЛЕТКИ ===

        public static BitmapImage Cell1 => Load("Assets\\cell\\cell1.png");
        public static BitmapImage Cell2 => Load("Assets\\cell\\cell2.png");
        public static BitmapImage Cell3 => Load("Assets\\cell\\cell3.png");
        public static BitmapImage Cell4 => Load("Assets\\cell\\cell4.png");
        public static BitmapImage Cell5 => Load("Assets\\cell\\cell5.png");
        public static BitmapImage Cell6 => Load("Assets\\cell\\cell6.png");
        public static BitmapImage Cell7 => Load("Assets\\cell\\cell7.png");
        public static BitmapImage Cell8 => Load("Assets\\cell\\cell8.png");

        public static BitmapImage CellBomb => Load("Assets\\cell\\cellBomb.png");
        public static BitmapImage CellBombClick => Load("Assets\\cell\\cellBombClick.png");
        public static BitmapImage CellBombWrong => Load("Assets\\cell\\cellBombWrong.png");

        public static BitmapImage CellClose => Load("Assets\\cell\\cellClose.png");
        public static BitmapImage CellFlag => Load("Assets\\cell\\cellFlag.png");
        public static BitmapImage CellIsEmpty => Load("Assets\\cell\\cellIsEmpty.png");
        public static BitmapImage CellUnknown => Load("Assets\\cell\\cellUnknown.png");
        public static BitmapImage CellUnknownOpen => Load("Assets\\cell\\cellUnknownOpen.png");

        // === ЛИЦА ===

        public static BitmapImage FaceLose => Load("Assets\\face\\faceLose.png");
        public static BitmapImage FaceSmile => Load("Assets\\face\\faceSmile.png");
        public static BitmapImage FaceSmileClick => Load("Assets\\face\\faceSmileClick.png");
        public static BitmapImage FaceWin => Load("Assets\\face\\faceWin.png");
        public static BitmapImage FaceWorried => Load("Assets\\face\\faceWorried.png");

        // === ЦИФРЫ (таймер/счётчик) ===

        public static BitmapImage NumberMinus => Load("Assets\\number\\number-.png");
        public static BitmapImage Number0 => Load("Assets\\number\\number0.png");
        public static BitmapImage Number1 => Load("Assets\\number\\number1.png");
        public static BitmapImage Number2 => Load("Assets\\number\\number2.png");
        public static BitmapImage Number3 => Load("Assets\\number\\number3.png");
        public static BitmapImage Number4 => Load("Assets\\number\\number4.png");
        public static BitmapImage Number5 => Load("Assets\\number\\number5.png");
        public static BitmapImage Number6 => Load("Assets\\number\\number6.png");
        public static BitmapImage Number7 => Load("Assets\\number\\number7.png");
        public static BitmapImage Number8 => Load("Assets\\number\\number8.png");
        public static BitmapImage Number9 => Load("Assets\\number\\number9.png");
        public static BitmapImage NumberIsEmpty => Load("Assets\\number\\numberIsEmpty.png");
    }
}