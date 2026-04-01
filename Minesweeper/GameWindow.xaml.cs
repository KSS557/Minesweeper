using System.Windows;
using System.Windows.Controls;

namespace Minesweeper
{
    public partial class GameWindow : Window
    {
        public GameWindow(int width, int height)
        {
            InitializeComponent();
            GenerateBoard(width, height);
        }

        public void GenerateBoard(int width, int height)
        {
            const int tileSize = 20;
            const int margin = 20;
            const int captionBar = 30;

            BoardGrid.RowDefinitions.Clear();
            BoardGrid.ColumnDefinitions.Clear();
            BoardGrid.Children.Clear();

            for (int r = 0; r < height; r++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(tileSize) });
            }

            for (int c = 0; c < width; c++)
            {
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tileSize) });
            }

            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var btn = new System.Windows.Controls.Button
                    {
                        Margin = new Thickness(0.5),
                        Background = System.Windows.Media.Brushes.AliceBlue,
                        BorderBrush = System.Windows.Media.Brushes.LightGray,
                        Padding = new Thickness(0)
                    };

                    System.Windows.Controls.Grid.SetRow(btn, r);
                    System.Windows.Controls.Grid.SetColumn(btn, c);

                    BoardGrid.Children.Add(btn);
                }
            }

            int boardWidth = width * tileSize;
            int boardHeight = height * tileSize;

            this.MaxWidth = boardWidth + margin + 7;
            this.MaxHeight = boardHeight + captionBar + margin + 7;

            this.Width = this.MaxWidth;
            this.Height = this.MaxHeight;
        }
    }
}