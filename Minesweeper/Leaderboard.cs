using Microsoft.Data.Sqlite;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Reflection;

namespace Minesweeper
{
    public class Leaderboard : IDisposable
    {
        private readonly string _filePath;
        private SqliteConnection _connection;

        public Leaderboard()
        {
            _filePath = Path.Combine(Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location)!, "leaderboard.db");
            CreateDatabase();
        }

        private void CreateDatabase()
        {
            _connection = new SqliteConnection($"Data Source={_filePath}");
            _connection.Open();

            string createTable = @"
                CREATE TABLE IF NOT EXISTS Leaderboards (
                    id INTEGER PRIMARY KEY AUTOINCREMENT,
                    nickname TEXT NOT NULL,
                    difficulty INTEGER NOT NULL,
                    time DATETIME NOT NULL,
                    date DATETIME NOT NULL
                )";

            using var cmd = new SqliteCommand(createTable, _connection);
            cmd.ExecuteNonQuery();
        }

        public void AddRecord(string nickname, int difficulty, TimeOnly time)
        {
            string insert = @"
                INSERT INTO Leaderboards (nickname, difficulty, time, date)
                VALUES (@nickname, @difficulty, @time, @date)";

            using var cmd = new SqliteCommand(insert, _connection);
            cmd.Parameters.AddWithValue("@nickname", nickname);
            cmd.Parameters.AddWithValue("@difficulty", difficulty);
            cmd.Parameters.AddWithValue("@time", time);
            cmd.Parameters.AddWithValue("@date", DateTime.Now);
            cmd.ExecuteNonQuery();
            Debug.WriteLine(cmd.Parameters);
        }

        public DataTable GetTopByDifficulty(int difficulty)
        {
            var table = new DataTable();
            string select = @"
                SELECT nickname, difficulty, time, date 
                FROM Leaderboards 
                WHERE difficulty = @diff 
                ORDER BY time ASC 
                LIMIT 100";

            using var cmd = new SqliteCommand(select, _connection);
            cmd.Parameters.AddWithValue("@diff", difficulty);

            using var reader = cmd.ExecuteReader();
            table.Load(reader);

            return table;
        }

        public void Dispose()
        {
            _connection?.Dispose();
        }
    }
}