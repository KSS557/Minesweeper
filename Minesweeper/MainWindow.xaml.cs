using System.Diagnostics;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly DifficultySettings _difficulty;
        public readonly GameBoardController _game;

        public MainWindow()
        {
            InitializeComponent();
            _difficulty = new DifficultySettings(this);
            _game = new GameBoardController(this);
        }

        public void ShowSettings()
        {
            PanelSettings.Visibility = Visibility.Visible;
            PanelGame.Visibility = Visibility.Collapsed;
            ResizeMode = ResizeMode.NoResize;

            if (Application.Current.MainWindow is Window w)
            {
                w.ResizeMode = ResizeMode.NoResize;
                ResizeFromCenter(250, 400);
            }
        }

        public int Width => _difficulty.Width;
        public int Height => _difficulty.Height;
        public int MineCount => _difficulty.MineCount;

        public void ResizeFromCenter(double newWidth, double newHeight)
        {
            


            if (Application.Current.MainWindow is Window w)
            {
                double oldCenterX = (w.Left + w.Width) / 2;
                double oldCenterY = (w.Top + w.Height) / 2;

                Debug.WriteLine($"Left: {w.Left}, Top: {w.Top}, oldCenterX: {oldCenterX}, oldCenterY: {oldCenterY}, Width: {w.Width} , Height: {w.Height}");

                w.Left = (oldCenterX - w.Width) / 2;
                w.Top = (oldCenterY - w.Height) / 2;

                Debug.WriteLine($"Left 2: {w.Left}, Top 2: {w.Top}, newWidth: {newWidth}, newHeight: {newHeight}");
                w.Width = newWidth;
                w.Height = newHeight;
            }
        }

        private void GameScrollViewer_PreviewMouseWheel(object sender, MouseWheelEventArgs e)
        {
            var scrollViewer = (ScrollViewer)sender;

            if (Keyboard.Modifiers == ModifierKeys.Shift)
            {
                double delta = 3.0 * e.Delta;
                double newOffset = scrollViewer.HorizontalOffset - delta;

                newOffset = Math.Max(0, Math.Min(newOffset, scrollViewer.ScrollableWidth));
                scrollViewer.ScrollToHorizontalOffset(newOffset);

                e.Handled = true;
            }
        }
    }
}