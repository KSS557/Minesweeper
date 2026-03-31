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

        private void GenerateBoard(int width, int height)
        {
            const int tileSize = 20; // размер одной клетки

            // Настраиваем Grid: столько строк и столбцов, сколько задано
            for (int r = 0; r < height; r++)
            {
                BoardGrid.RowDefinitions.Add(new RowDefinition { Height = new GridLength(tileSize) });
            }

            for (int c = 0; c < width; c++)
            {
                BoardGrid.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(tileSize) });
            }

            // Пример: заполняем кнопками‑клетками (можно заменить на Label / другой Control)
            for (int r = 0; r < height; r++)
            {
                for (int c = 0; c < width; c++)
                {
                    var btn = new System.Windows.Controls.Button
                    {
                        Margin = new Thickness(0.5), // тонкие отступы между клетками
                        Background = System.Windows.Media.Brushes.AliceBlue,
                        BorderBrush = System.Windows.Media.Brushes.LightGray,
                        Padding = new Thickness(0)
                    };

                    // Пример: пишешь координаты в Content для отладки
                    //btn.Content = $"{r},{c}";

                    System.Windows.Controls.Grid.SetRow(btn, r);
                    System.Windows.Controls.Grid.SetColumn(btn, c);

                    BoardGrid.Children.Add(btn);
                }
            }

            // Задаём примерный размер окна под доску (дополнительно можно корректировать)
            Width = width * tileSize + 20;
            Height = height * tileSize + 50; // + шапка и отступы
        }
    }
}