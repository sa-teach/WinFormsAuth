using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace WinFormsAuth
{
    internal static class DatabaseHelper
    {
        private static readonly string DbPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.db");

        private static string ConnectionString => $"Data Source={DbPath}";

        public static void Initialize()
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"
                CREATE TABLE IF NOT EXISTS Пользователи (
                    Id             INTEGER PRIMARY KEY AUTOINCREMENT,
                    Login          TEXT    NOT NULL UNIQUE,
                    Password       TEXT    NOT NULL,
                    Role           TEXT    NOT NULL DEFAULT 'Пользователь',
                    IsBlocked      INTEGER NOT NULL DEFAULT 0,
                    FailedAttempts INTEGER NOT NULL DEFAULT 0
                )";
            cmd.ExecuteNonQuery();

            SeedUser("admin", "admin123", "Администратор");
            SeedUser("user1", "user123", "Пользователь");
        }

        private static void SeedUser(string login, string password, string role)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT OR IGNORE INTO Пользователи (Login, Password, Role)
                                VALUES (@login, @password, @role)";
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@password", HashPassword(password));
            cmd.Parameters.AddWithValue("@role", role);
            cmd.ExecuteNonQuery();
        }

        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        public record UserRecord(int Id, string Login, string Role, bool IsBlocked, int FailedAttempts);

        public static UserRecord? GetUser(string login)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Login, Role, IsBlocked, FailedAttempts FROM Пользователи WHERE Login = @login";
            cmd.Parameters.AddWithValue("@login", login);
            using var reader = cmd.ExecuteReader();
            if (!reader.Read()) return null;
            return new UserRecord(
                reader.GetInt32(0),
                reader.GetString(1),
                reader.GetString(2),
                reader.GetInt32(3) != 0,
                reader.GetInt32(4));
        }

        public static bool ValidatePassword(string login, string password)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT Password FROM Пользователи WHERE Login = @login";
            cmd.Parameters.AddWithValue("@login", login);
            var stored = cmd.ExecuteScalar() as string;
            return stored == HashPassword(password);
        }

        public static void IncrementFailedAttempts(string login)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"UPDATE Пользователи
                SET FailedAttempts = FailedAttempts + 1,
                    IsBlocked = CASE WHEN FailedAttempts + 1 >= 3 THEN 1 ELSE IsBlocked END
                WHERE Login = @login";
            cmd.Parameters.AddWithValue("@login", login);
            cmd.ExecuteNonQuery();
        }

        public static void ResetFailedAttempts(string login)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Пользователи SET FailedAttempts = 0 WHERE Login = @login";
            cmd.Parameters.AddWithValue("@login", login);
            cmd.ExecuteNonQuery();
        }

        public static List<UserRecord> GetAllUsers()
        {
            var list = new List<UserRecord>();
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "SELECT Id, Login, Role, IsBlocked, FailedAttempts FROM Пользователи ORDER BY Id";
            using var reader = cmd.ExecuteReader();
            while (reader.Read())
                list.Add(new UserRecord(
                    reader.GetInt32(0),
                    reader.GetString(1),
                    reader.GetString(2),
                    reader.GetInt32(3) != 0,
                    reader.GetInt32(4)));
            return list;
        }

        public static bool LoginExists(string login)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Пользователи WHERE Login = @login";
            cmd.Parameters.AddWithValue("@login", login);
            return Convert.ToInt64(cmd.ExecuteScalar()!) > 0;
        }

        public static void AddUser(string login, string password, string role)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO Пользователи (Login, Password, Role) VALUES (@login, @password, @role)";
            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@password", HashPassword(password));
            cmd.Parameters.AddWithValue("@role", role);
            cmd.ExecuteNonQuery();
        }

        public static void UpdateUser(int id, string login, string? newPassword, string role, bool isBlocked)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();

            if (newPassword != null)
            {
                cmd.CommandText = @"UPDATE Пользователи
                    SET Login=@login, Password=@password, Role=@role,
                        IsBlocked=@blocked, FailedAttempts=@fa
                    WHERE Id=@id";
                cmd.Parameters.AddWithValue("@password", HashPassword(newPassword));
            }
            else
            {
                cmd.CommandText = @"UPDATE Пользователи
                    SET Login=@login, Role=@role,
                        IsBlocked=@blocked, FailedAttempts=@fa
                    WHERE Id=@id";
            }

            cmd.Parameters.AddWithValue("@login", login);
            cmd.Parameters.AddWithValue("@role", role);
            cmd.Parameters.AddWithValue("@blocked", isBlocked ? 1 : 0);
            cmd.Parameters.AddWithValue("@fa", isBlocked ? 3 : 0);
            cmd.Parameters.AddWithValue("@id", id);
            cmd.ExecuteNonQuery();
        }
    }
}
