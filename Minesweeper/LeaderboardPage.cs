using System;
using System.Collections.Generic;
using System.Data;
using System.Diagnostics;
using System.Text;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

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


            _window.BtnEasy.Click += DifficultyButton_Click;
            _window.BtnMedium.Click += DifficultyButton_Click;
            _window.BtnHard.Click += DifficultyButton_Click;
            _window.BtnCustom.Click += DifficultyButton_Click;

            LoadLeaderboard(1);
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
            _window._leaderboard.AddRecord(nickname, _difficulty, gameTime);

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

        private void DifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {
                

                //btn.Background = new SolidColorBrush(Colors.LightBlue);  // или ваш стиль
                //btn.IsEnabled = false;  // визуально "активна"

                string difficultyTag = btn.Tag.ToString();
                int difficulty = GetDifficultyFromTag(difficultyTag);

                LoadLeaderboard(difficulty);
            }
        }

        private int GetDifficultyFromTag(string tag)
        {
            return tag switch
            {
                "Easy" => 1,
                "Medium" => 2,
                "Hard" => 3,
                "Custom" => 4,
                _ => 1
            };
        }

        private void LoadLeaderboard(int difficulty)
        {
            try
            {
                var leaderboard = new Leaderboard();  // ваш класс
                DataTable table = leaderboard.GetTopByDifficulty(difficulty);

                // ✅ Привязка DataTable к DataGrid
                _window.LeaderboardTable.ItemsSource = table.DefaultView;

                // ✅ Добавляем колонку Rank (номер места)
                if (!table.Columns.Contains("Rank"))
                {
                    table.Columns.Add("Rank", typeof(int));
                    for (int i = 0; i < table.Rows.Count; i++)
                    {
                        table.Rows[i]["Rank"] = i + 1;
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки лидерборда: {ex.Message}");
            }
        }
    }
}

