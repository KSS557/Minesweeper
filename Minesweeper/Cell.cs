using System.Windows.Controls;
using System.Linq;

namespace Minesweeper
{
    public class Cell
    {
        public Image Img { get; set; }
        public int Row { get; set; }
        public int Col { get; set; }
        public bool IsMine { get; set; }
        public int AdjacentMines { get; set; }
        public bool IsOpened { get; set; } = false;  // По умолчанию closed=false? Нет, true=closed? Исправьте логику
        public bool IsFlagged { get; set; }
        public bool HasNumber => AdjacentMines > 0 && AdjacentMines <= 8 && !IsOpened;

        public List<Cell> GetNeighbors(GameBoardController controller, int totalRows, int totalCols)
        {
            var neighbors = new List<Cell>();
            for (int dr = -1; dr <= 1; dr++)
            {
                for (int dc = -1; dc <= 1; dc++)
                {
                    if (dr == 0 && dc == 0) continue;
                    int nr = Row + dr, nc = Col + dc;
                    if (nr >= 0 && nr < totalRows && nc >= 0 && nc < totalCols)
                    {
                        int idx = nr * totalCols + nc;
                        var neighborImg = controller._window.BoardCanvas.Children.OfType<Image>()
                            .FirstOrDefault(i => i.Name == "cell" + idx);
                        if (neighborImg?.Tag is Cell n) neighbors.Add(n);
                    }
                }
            }
            return neighbors;
        }
    }
}