using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    public class DifficultySettings : INotifyPropertyChanged
    {
        private readonly MainWindow _window;
        private int _width;
        private int _height;
        private int _mineCount;
        private const int MinSize = 2;
        private const int MaxSize = 50;

        public int Width
        {
            get => _width;
            set
            {
                if (_width != value)
                {
                    _width = value;
                    OnPropertyChanged(nameof(Width));
                }
            }
        }

        public int Height
        {
            get => _height;
            set
            {
                if (_height != value)
                {
                    _height = value;
                    OnPropertyChanged(nameof(Height));
                }
            }
        }

        public int MineCount
        {
            get => _mineCount;
            set
            {
                if (_mineCount != value)
                {
                    _mineCount = value;
                    OnPropertyChanged(nameof(MineCount));
                }
            }
        }

        public DifficultySettings(MainWindow window)
        {
            _window = window;

            _window.Easy.Checked += OnDifficultyChanged;
            _window.Medium.Checked += OnDifficultyChanged;
            _window.Hard.Checked += OnDifficultyChanged;
            _window.Custom.Checked += OnDifficultyChanged;

            _window.TxtWidth.LostFocus += OnSizeUpdated;
            _window.TxtWidth.KeyUp += OnSizeKeyUp;
            _window.TxtHeight.LostFocus += OnSizeUpdated;
            _window.TxtHeight.KeyUp += OnSizeKeyUp;
            _window.Mine.LostFocus += OnMineUpdated;
            _window.Mine.KeyUp += OnMineKeyUp;

            _window.TxtWidth.PreviewTextInput += TextBoxOnlyDigits;
            _window.TxtHeight.PreviewTextInput += TextBoxOnlyDigits;
            _window.Mine.PreviewTextInput += TextBoxOnlyDigits;

            _window.TxtWidth.GotFocus += TextBoxSelectAll;
            _window.TxtHeight.GotFocus += TextBoxSelectAll;
            _window.Mine.GotFocus += TextBoxSelectAll;

            SetEasy();

            _window.BtnPlay.Click += (sender, e) =>
            {
                int difficultyLevel = _window.Easy.IsChecked == true ? 1 :
                         _window.Medium.IsChecked == true ? 2 :
                         _window.Hard.IsChecked == true ? 3 : 4;

                _window._game.StartGame(_width, _height, _mineCount);
                _window._leaderboardPage.GiveDifficulty(difficultyLevel);
                _window.PanelSettings.Visibility = Visibility.Collapsed;
            };
        }

        private void TextBoxSelectAll(object sender, RoutedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                tb.SelectAll();
            }
        }

        private void OnDifficultyChanged(object sender, RoutedEventArgs e)
        {
            if (_window.Easy.IsChecked == true)
            {
                SetEasy();
            }
            else if (_window.Medium.IsChecked == true)
            {
                SetMedium();
            }
            else if (_window.Hard.IsChecked == true)
            {
                SetHard();
            }
            else if (_window.Custom.IsChecked == true)
            {
                SetCustomInputsEnabled(true);
                if (string.IsNullOrWhiteSpace(_window.TxtWidth.Text))
                    _window.TxtWidth.Text = "2";
                if (string.IsNullOrWhiteSpace(_window.TxtHeight.Text))
                    _window.TxtHeight.Text = "2";
                if (string.IsNullOrWhiteSpace(_window.Mine.Text))
                    _window.Mine.Text = "1";
            }
        }

        private void SetEasy()
        {
            Width = 9;
            Height = 9;
            MineCount = 10;
            SetTextBoxes(Width, Height, MineCount, false);
        }

        private void SetMedium()
        {
            Width = 16;
            Height = 16;
            MineCount = 40;
            SetTextBoxes(Width, Height, MineCount, false);
        }

        private void SetHard()
        {
            Width = 30;
            Height = 16;
            MineCount = 99;
            SetTextBoxes(Width, Height, MineCount, false);
        }

        private void SetTextBoxes(int w, int h, int m, bool enabled)
        {
            _window.TxtWidth.Text = w.ToString();
            _window.TxtHeight.Text = h.ToString();
            _window.Mine.Text = m.ToString();
            _window.TxtWidth.IsEnabled = enabled;
            _window.TxtHeight.IsEnabled = enabled;
            _window.Mine.IsEnabled = enabled;
        }

        private void SetCustomInputsEnabled(bool enabled)
        {
            _window.TxtWidth.IsEnabled = enabled;
            _window.TxtHeight.IsEnabled = enabled;
            _window.Mine.IsEnabled = enabled;
        }

        private void SwitchFocusToNextBox(TextBox current)
        {
            if (current == _window.TxtWidth)
            {
                _window.TxtHeight.Focus();
            }
            else if (current == _window.TxtHeight)
            {
                _window.Mine.Focus();
            }
            else if (current == _window.Mine)
            {
                Keyboard.ClearFocus();
            }
        }

        private void OnSizeUpdated(object sender, RoutedEventArgs e) => UpdateFromTextBox();
        private void OnSizeKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateFromTextBox();

                if (sender is TextBox current)
                    SwitchFocusToNextBox(current);
            }
        }

        private void OnMineUpdated(object sender, RoutedEventArgs e) => UpdateMineFromTextBox();
        private void OnMineKeyUp(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                UpdateMineFromTextBox();

                if (sender is TextBox current)
                    SwitchFocusToNextBox(current);
            }
        }

        private void UpdateFromTextBox()
        {
            UpdateWidthHeight();
            UpdateMineFromTextBox();
        }

        private void UpdateWidthHeight()
        {
            Width = _window.TxtWidth.Text switch
            {
                null or "" => MinSize,
                _ when int.TryParse(_window.TxtWidth.Text, out int w) => w,
                _ => MinSize
            };

            Height = _window.TxtHeight.Text switch
            {
                null or "" => MinSize,
                _ when int.TryParse(_window.TxtHeight.Text, out int h) => h,
                _ => MinSize
            };

            Width = Math.Clamp(Width, MinSize, MaxSize);
            Height = Math.Clamp(Height, MinSize, MaxSize);

            _window.TxtWidth.Text = Width.ToString();
            _window.TxtHeight.Text = Height.ToString();
        }

        private void UpdateMineFromTextBox()
        {
            if (Width < MinSize) Width = MinSize;
            if (Height < MinSize) Height = MinSize;
            if (Width > MaxSize) Width = MaxSize;
            if (Height > MaxSize) Height = MaxSize;

            int maxMines = Width * Height - 1;

            MineCount = _window.Mine.Text switch
            {
                null or "" => 1,
                _ when int.TryParse(_window.Mine.Text, out int m) => m,
                _ => 1
            };

            if (MineCount < 1)
                MineCount = 1;
            if (MineCount > maxMines)
                MineCount = maxMines;

            _window.Mine.Text = MineCount.ToString();
        }

        private void TextBoxOnlyDigits(object sender, TextCompositionEventArgs e)
        {
            e.Handled = !Regex.IsMatch(e.Text, "^[0-9]+$");
        }

        public event PropertyChangedEventHandler? PropertyChanged;
        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}