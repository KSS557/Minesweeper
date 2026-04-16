using System.Data;
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
        public string _nickname = null;

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
        }

        private void BtnSaveRecord_Click(object sender, RoutedEventArgs e)
        {
            string nickname = _window.TextBoxLeaderboard.Text;
            int difficulty = _difficulty;
            TimeOnly gameTime = _window._game.Time;
            string timeString = gameTime.ToString("HH:mm:ss.ff");

            if (nickname.Length < 4)
            {
                MessageBox.Show("Введите никнейм! Больше 4 символов");
                return;
            }
            _window._leaderboard.AddRecord(nickname, difficulty, timeString);

            _window.TextBoxLeaderboard.Clear();

            _window.HideLeaderboardOverlay();
            _window.ShowSettings();
            _window.ShowLeaderbord();
            _nickname = null;
            LoadLeaderboard(difficulty);
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

        public void GiveDifficulty(int difficulty)
        {
            _difficulty = difficulty;
        }

        private void DifficultyButton_Click(object sender, RoutedEventArgs e)
        {
            if (sender is Button btn)
            {

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


        public void LoadLeaderboard(int difficulty)
        {
            try
            {
                _difficulty = difficulty;
                var leaderboard = new Leaderboard();
                DataTable table;
                if (_nickname == null)
                {
                    table = leaderboard.GetTopByDifficulty(difficulty);
                }
                else
                {
                    table = leaderboard.GetPlayerRecords(_nickname, difficulty);
                }
                _window.LeaderboardTable.ItemsSource = table.DefaultView;

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

        public void LeaderboardNickname(string nickname)
        {
            _nickname = nickname;
            LoadLeaderboard(_difficulty);

        }
    }
}

