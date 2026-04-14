using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Minesweeper
{
    public class LeaderboardPage
    {
        private readonly MainWindow _window;
        private readonly Leaderboard _leaderboard;
        public TextBox TextBoxLeaderboard => _window.TextBoxLeaderboard;

        private int _difficulty;

        public LeaderboardPage(MainWindow window)
        {
            _window = window;
            _leaderboard = new Leaderboard();

            var tb = _window.TextBoxLeaderboard;

            tb.KeyDown += TextBoxLeaderboard_KeyDown;
            tb.TextChanged += TextBoxLeaderboard_TextChanged;
            tb.PreviewTextInput += TextBoxLeaderboard_PreviewTextInput;
            _window.PreviewMouseDown += Window_PreviewMouseDown;

            _window.BtnLeaderboardSave.Click += BtnSaveRecord_Click;
        }

        private void BtnSaveRecord_Click(object sender, RoutedEventArgs e)
        {
            string nickname = _window.TextBoxLeaderboard.Text;
            int difficulty = _difficulty;
            TimeOnly gameTime = _window._game.Time;

            if (nickname.Length < 4)
            {
                MessageBox.Show("Введите никнейм! Больше 4 символов");
                return;
            }
            Debug.WriteLine($"{nickname}, {difficulty}, {gameTime}, {DateTime.Now}");
            _leaderboard.AddRecord(nickname, _difficulty, gameTime);

            _window.TextBoxLeaderboard.Clear();

            _window.ShowSettings();
            _window.ShowLeaderbord();
        }

        private void Window_PreviewMouseDown(object sender, MouseButtonEventArgs e)
        {
            if (_window.TextBoxLeaderboard.IsFocused && !(e.OriginalSource is TextBox) && e.OriginalSource != _window.TextBoxLeaderboard)
            {
                _window.BtnLeaderboardSave.Focus();
                e.Handled = false;
            }
        }

        private void TextBoxLeaderboard_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                if (sender is TextBox tb)
                {
                    tb.MoveFocus(new TraversalRequest(FocusNavigationDirection.Next));
                    e.Handled = true;
                }
            }
        }

        private void TextBoxLeaderboard_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (sender is TextBox tb)
            {
                if (tb.Text.Length > 15)
                {
                    tb.Text = tb.Text[..15];
                    tb.CaretIndex = 15;
                }
            }
        }

        private void TextBoxLeaderboard_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (sender is TextBox tb)
            {
                string pattern = @"^[a-zA-Z0-9_.\-\s]{0,15}$";
                e.Handled = !Regex.IsMatch(e.Text, @"[a-zA-Z0-9_.\-\s]") ||
                            tb.Text.Length + e.Text.Length > 15;
            }
        }

        public void LoadLeaderboard(string difficulty)
        {
            // _window.LeaderboardTable.ItemsSource = ...
        }

        public void GiveDifficulty(int difficulty)
        {
            _difficulty = difficulty;
        }

        public void Unload()
        {
            _window.TextBoxLeaderboard.KeyDown -= TextBoxLeaderboard_KeyDown;
            _window.TextBoxLeaderboard.TextChanged -= TextBoxLeaderboard_TextChanged;
        }
    }
}

