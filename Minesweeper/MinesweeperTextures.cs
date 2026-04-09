using System;
using System.Windows.Media.Imaging;

namespace Minesweeper
{
    public static class MinesweeperTextures
    {
        private static Dictionary<string, BitmapImage> _cache = new();

        private static BitmapImage Load(string path) =>
            new BitmapImage(new Uri($"pack://application:,,,/Assets/{path}.png"));

        private static BitmapImage Cell(string name) => Load($"cell\\{name}");
        private static BitmapImage Face(string name) => Load($"face\\{name}");
        private static BitmapImage Number(string name) => Load($"number\\{name}");

        // === КЛЕТКИ ===

        public static BitmapImage Cell1 => Cell("cell1");
        public static BitmapImage Cell2 => Cell("cell2");
        public static BitmapImage Cell3 => Cell("cell3");
        public static BitmapImage Cell4 => Cell("cell4");
        public static BitmapImage Cell5 => Cell("cell5");
        public static BitmapImage Cell6 => Cell("cell6");
        public static BitmapImage Cell7 => Cell("cell7");
        public static BitmapImage Cell8 => Cell("cell8");

        public static BitmapImage CellBomb => Cell("cellBomb");
        public static BitmapImage CellBombClick => Cell("cellBombClick");
        public static BitmapImage CellBombWrong => Cell("cellBombWrong");

        public static BitmapImage CellClose => Cell("cellClose");
        public static BitmapImage CellFlag => Cell("cellFlag");
        public static BitmapImage CellIsEmpty => Cell("cellIsEmpty");
        public static BitmapImage CellUnknown => Cell("cellUnknown");
        public static BitmapImage CellUnknownOpen => Cell("cellUnknownOpen");

        // === ЛИЦА ===

        public static BitmapImage FaceLose => Face("faceLose");
        public static BitmapImage FaceSmile => Face("faceSmile");
        public static BitmapImage FaceSmileClick => Face("faceSmileClick");
        public static BitmapImage FaceWin => Face("faceWin");
        public static BitmapImage FaceWorried => Face("faceWorried");

        // === ЦИФРЫ ===

        public static BitmapImage NumberMinus => Number("number-");
        public static BitmapImage Number0 => Number("number0");
        public static BitmapImage Number1 => Number("number1");
        public static BitmapImage Number2 => Number("number2");
        public static BitmapImage Number3 => Number("number3");
        public static BitmapImage Number4 => Number("number4");
        public static BitmapImage Number5 => Number("number5");
        public static BitmapImage Number6 => Number("number6");
        public static BitmapImage Number7 => Number("number7");
        public static BitmapImage Number8 => Number("number8");
        public static BitmapImage Number9 => Number("number9");
        public static BitmapImage NumberIsEmpty => Number("numberIsEmpty");
    }
}