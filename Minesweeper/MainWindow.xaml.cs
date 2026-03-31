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
        private const int MinSize = 2;
        private const int MaxSize = 99;

        public MainWindow()
        {
            InitializeComponent();
            new DifficultySettings(this);
        }
    }
}