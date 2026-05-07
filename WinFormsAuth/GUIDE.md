# Гайд: WinForms приложение с авторизацией, ролями и пазл-капчей

Пошаговая инструкция по созданию приложения с нуля. Все действия выполняются вручную через Visual Studio.

---



## Содержание

1. [Создание проекта](#1-создание-проекта)
2. [Подключение SQLite](#2-подключение-sqlite)
3. [Подготовка изображений для пазла](#3-подготовка-изображений-для-пазла)
4. [Создание класса DatabaseHelper](#4-создание-класса-databasehelper)
5. [Форма авторизации — интерфейс](#5-форма-авторизации--интерфейс)
6. [Форма авторизации — капча](#6-форма-авторизации--капча)
7. [Форма авторизации — логика входа](#7-форма-авторизации--логика-входа)
8. [Форма администратора — интерфейс](#8-форма-администратора--интерфейс)
9. [Форма администратора — логика](#9-форма-администратора--логика)
10. [Форма пользователя](#10-форма-пользователя)
11. [Точка входа Program.cs](#11-точка-входа-programcs)
12. [Адаптация под свою базу данных](#12-адаптация-под-свою-базу-данных)
13. [Проверка работы](#13-проверка-работы)

---

## 1. Создание проекта

1. Запустите Visual Studio → **Создать новый проект**
2. Выберите шаблон **Windows Forms App (.NET)** (не Framework!)
3. Введите имя проекта, например `WinFormsAuth`
4. Выберите версию **.NET 8** или выше → **Создать**

> **📸 Место для скриншота:** окно выбора шаблона проекта

После создания у вас будет `Form1.cs`. Переименуйте её:
- В Solution Explorer правой кнопкой на `Form1.cs` → **Переименовать** → `AuthForm.cs`
- Visual Studio спросит переименовать класс тоже — нажмите **Да**

---

## 2. Подключение SQLite

Нам нужен пакет `Microsoft.Data.Sqlite`. Он не требует установки СУБД.

**Способ 1 — через интерфейс Visual Studio:**

1. В Solution Explorer правой кнопкой на проект → **Управление пакетами NuGet**
2. Вкладка **Обзор** → в поиске введите `Microsoft.Data.Sqlite`
3. Выберите пакет от Microsoft → **Установить**

> **📸 Место для скриншота:** NuGet Manager с найденным пакетом

**Способ 2 — через консоль:**

Меню **Инструменты → Диспетчер пакетов NuGet → Консоль диспетчера пакетов**, затем:

```
Install-Package Microsoft.Data.Sqlite
```

После установки в файле `.csproj` появится строка:
```xml
<PackageReference Include="Microsoft.Data.Sqlite" Version="9.0.x" />
```

---

## 3. Пазл

Пазл состоит из **4 отдельных картинок**, которые нужно перемешать и сложить в правильном порядке.

### Требования к изображениям

- **Количество:** ровно 4 файла
- **Имена:** `1.png`, `2.png`, `3.png`, `4.png`
- **Смысл:** вместе они должны образовывать одну картинку или логический ряд, чтобы пользователь понимал правильный порядок

### Добавление изображений в проект

1. В Solution Explorer создайте папку: правой кнопкой на проект → **Добавить → Новая папка** → назовите `Img`
2. Правой кнопкой на папку `Img` → **Добавить → Существующий элемент**
3. Выберите все 4 файла сразу

> **📸 Место для скриншота:** папка Img с файлами в Solution Explorer

## 4. Создание класса DatabaseHelper

Этот класс будет отвечать за всю работу с базой данных.

Правой кнопкой на проект → **Добавить → Класс** → имя `DatabaseHelper.cs`.

Вставьте содержимое целиком:

```csharp
using Microsoft.Data.Sqlite;
using System.Security.Cryptography;
using System.Text;

namespace ВашНеймспейс   // замените на имя вашего проекта
{
    internal static class DatabaseHelper
    {
        // Файл базы данных будет создан рядом с .exe
        private static readonly string DbPath =
            Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "users.db");

        private static string ConnectionString => $"Data Source={DbPath}";

        // Вызывается один раз при старте - создаёт таблицу и тестовых пользователей
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
            SeedUser("user1", "user123",  "Пользователь");
        }

        // Добавляет пользователя только если его ещё нет
        private static void SeedUser(string login, string password, string role)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = @"INSERT OR IGNORE INTO Пользователи (Login, Password, Role)
                                VALUES (@login, @password, @role)";
            cmd.Parameters.AddWithValue("@login",    login);
            cmd.Parameters.AddWithValue("@password", HashPassword(password));
            cmd.Parameters.AddWithValue("@role",     role);
            cmd.ExecuteNonQuery();
        }

        // SHA-256 хеш пароля
        public static string HashPassword(string password)
        {
            var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(password));
            return Convert.ToHexString(bytes).ToLower();
        }

        // Удобная запись для хранения данных пользователя
        public record UserRecord(int Id, string Login, string Role,
                                 bool IsBlocked, int FailedAttempts);

        // Найти пользователя по логину (null если не найден)
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
                reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                reader.GetInt32(3) != 0, reader.GetInt32(4));
        }

        // Проверить пароль
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

        // +1 к счётчику неудач; автоматически блокирует при >= 3
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

        // Сброс счётчика после успешного входа
        public static void ResetFailedAttempts(string login)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "UPDATE Пользователи SET FailedAttempts = 0 WHERE Login = @login";
            cmd.Parameters.AddWithValue("@login", login);
            cmd.ExecuteNonQuery();
        }

        // Все пользователи (для таблицы в AdminForm)
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
                    reader.GetInt32(0), reader.GetString(1), reader.GetString(2),
                    reader.GetInt32(3) != 0, reader.GetInt32(4)));
            return list;
        }

        // Проверить, существует ли логин
        public static bool LoginExists(string login)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText = "SELECT COUNT(*) FROM Пользователи WHERE Login = @login";
            cmd.Parameters.AddWithValue("@login", login);
            return Convert.ToInt64(cmd.ExecuteScalar()!) > 0;
        }

        // Добавить нового пользователя
        public static void AddUser(string login, string password, string role)
        {
            using var conn = new SqliteConnection(ConnectionString);
            conn.Open();
            using var cmd = conn.CreateCommand();
            cmd.CommandText =
                "INSERT INTO Пользователи (Login, Password, Role) VALUES (@login, @password, @role)";
            cmd.Parameters.AddWithValue("@login",    login);
            cmd.Parameters.AddWithValue("@password", HashPassword(password));
            cmd.Parameters.AddWithValue("@role",     role);
            cmd.ExecuteNonQuery();
        }

        // Обновить данные пользователя (newPassword = null — не менять пароль)
        public static void UpdateUser(int id, string login, string? newPassword,
                                      string role, bool isBlocked)
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
            cmd.Parameters.AddWithValue("@login",   login);
            cmd.Parameters.AddWithValue("@role",    role);
            cmd.Parameters.AddWithValue("@blocked", isBlocked ? 1 : 0);
            cmd.Parameters.AddWithValue("@fa",      isBlocked ? 3 : 0); // сброс при разблокировке
            cmd.Parameters.AddWithValue("@id",      id);
            cmd.ExecuteNonQuery();
        }
    }
}
```

---

## 5. Форма авторизации — интерфейс

Откройте `AuthForm.cs` в **режиме конструктора** (двойной клик в Solution Explorer).

### Размер формы

Кликните на свободное место формы. В окне Свойства (F4):
- `Size` → `460; 555`
- `Text` → `Авторизация`
- `FormBorderStyle` → `FixedSingle`
- `MaximizeBox` → `False`
- `StartPosition` → `CenterScreen`

### Добавляемые элементы

Перетащите из **Панели элементов** (Toolbox) следующие контролы и настройте свойства:

---

#### Label — заголовок

| Свойство | Значение |
|----------|----------|
| `Name` | `lblTitle` |
| `Text` | `Авторизация` |
| `Font` | Segoe UI, 18pt, Bold |
| `Size` | `460; 40` |
| `Location` | `0; 14` |
| `TextAlign` | `MiddleCenter` |

---

#### Label + TextBox — поле «Логин»

**Label:**

| Свойство | Значение |
|----------|----------|
| `Name` | `lblLogin` |
| `Text` | `Логин:` |
| `Size` | `80; 23` |
| `Location` | `40; 80` |
| `TextAlign` | `MiddleRight` |

**TextBox:**

| Свойство | Значение |
|----------|----------|
| `Name` | `txtLogin` |
| `Size` | `280; 26` |
| `Location` | `130; 77` |
| `Font` | Segoe UI, 10pt |

---

#### Label + TextBox — поле «Пароль»

**Label:**

| Свойство | Значение |
|----------|----------|
| `Name` | `lblPassword` |
| `Text` | `Пароль:` |
| `Size` | `80; 23` |
| `Location` | `40; 120` |
| `TextAlign` | `MiddleRight` |

**TextBox:**

| Свойство | Значение |
|----------|----------|
| `Name` | `txtPassword` |
| `Size` | `280; 26` |
| `Location` | `130; 117` |
| `Font` | Segoe UI, 10pt |
| `PasswordChar` | `*` |

---

#### Label — подсказка капчи

| Свойство | Значение |
|----------|----------|
| `Name` | `lblHint` |
| `Text` | `Сложите изображение (кликните две части, чтобы поменять местами):` |
| `Size` | `380; 22` |
| `Location` | `40; 158` |
| `AutoSize` | `False` |

---

#### Panel — контейнер пазла

> Это самый важный контрол для капчи. Ячейки внутри него будут добавлены **кодом**, а не через конструктор.

| Свойство | Значение |
|----------|----------|
| `Name` | `pnlCaptcha` |
| `Size` | `260; 260` |
| `Location` | `100; 185` |

---

#### Button — «Перемешать»

| Свойство | Значение |
|----------|----------|
| `Name` | `btnShuffle` |
| `Text` | `Перемешать` |
| `Size` | `200; 28` |
| `Location` | `130; 458` |

---

#### Button — «Войти»

| Свойство | Значение |
|----------|----------|
| `Name` | `btnEnter` |
| `Text` | `Войти` |
| `Size` | `200; 36` |
| `Location` | `130; 498` |
| `Font` | Segoe UI, 11pt, Bold |
| `BackColor` | `SteelBlue` (из списка Web) |
| `ForeColor` | `White` |
| `FlatStyle` | `Flat` |

> **📸 Место для скриншота:** итоговый вид формы в конструкторе

---

## 6. Форма авторизации — капча

Капча реализована **полностью в коде** (не через конструктор): 4 панели с картинками создаются программно и добавляются в `pnlCaptcha`, который вы уже разместили на форме.

### Как это работает

- Загружаются 4 картинки из папки `Img`
- Они перемешиваются и отображаются в сетке 2×2
- Пользователь кликает на два фрагмента — они меняются местами
- Система проверяет: стоят ли фрагменты в порядке 1→2→3→4

### Код

Откройте `AuthForm.cs` (не Designer!) и добавьте в класс поля и методы:

```csharp
// ── Поля для капчи (добавьте в начало класса) ──────────────────────────────

private Bitmap[]? _pieces;                          // 4 картинки-фрагмента
private readonly int[] _arrangement = [0, 1, 2, 3]; // текущий порядок фрагментов
private int _selectedCell = -1;                     // какая ячейка выбрана (-1 = никакая)

private readonly Panel[]      _cellPanels   = new Panel[4];    // 4 панели-ячейки
private readonly PictureBox[] _cellPictures = new PictureBox[4]; // 4 картинки внутри

// ── Инициализация капчи (вызовите из конструктора формы) ───────────────────

private void InitializeCaptcha()
{
    // Координаты 4 ячеек в сетке 2×2 внутри pnlCaptcha
    int[] xs = new int[] { 0, 130, 0,   130 };
    int[] ys = new int[] { 0, 0,   130, 130 };

    for (int i = 0; i < 4; i++)
    {
        int ci = i; // важно: сохраняем i в отдельную переменную для замыкания

        // Создаём панель-ячейку
        _cellPanels[i] = new Panel
        {
            Location  = new Point(xs[i], ys[i]),
            Size      = new Size(120, 120),
            BackColor = Color.LightGray,
            Cursor    = Cursors.Hand
        };

        // Создаём PictureBox внутри ячейки
        _cellPictures[i] = new PictureBox
        {
            Location = new Point(3, 3),
            Size     = new Size(114, 114),
            SizeMode = PictureBoxSizeMode.StretchImage,
            Cursor   = Cursors.Hand
        };

        // Клик на картинку или на панель — вызывает CellClick
        _cellPictures[i].Click += (_, _) => CellClick(ci);
        _cellPanels[i].Click   += (_, _) => CellClick(ci);

        _cellPanels[i].Controls.Add(_cellPictures[i]);
        pnlCaptcha.Controls.Add(_cellPanels[i]);
    }

    // Загружаем 4 картинки из папки Img (рядом с .exe)
    string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Img");
    _pieces = new Bitmap[4];
    for (int i = 0; i < 4; i++)
        _pieces[i] = new Bitmap(Path.Combine(imgDir, $"{i + 1}.png"));

    ShufflePuzzle(); // перемешать при открытии формы
}

// ── Перемешивание ──────────────────────────────────────────────────────────

private void ShufflePuzzle()
{
    var rng = new Random();
    // Перемешиваем, пока случайно не получили правильный порядок
    do
    {
        for (int i = 3; i > 0; i--)
        {
            int j = rng.Next(i + 1);
            (_arrangement[i], _arrangement[j]) = (_arrangement[j], _arrangement[i]);
        }
    } while (IsCaptchaSolved());

    _selectedCell = -1;
    UpdatePuzzleDisplay();
}

// ── Обновление картинок в ячейках ─────────────────────────────────────────

private void UpdatePuzzleDisplay()
{
    if (_pieces == null) return;
    for (int i = 0; i < 4; i++)
    {
        _cellPictures[i].Image   = _pieces[_arrangement[i]];
        _cellPanels[i].BackColor = (i == _selectedCell) ? Color.Gold : Color.LightGray;
    }
}

// ── Клик по ячейке ─────────────────────────────────────────────────────────

private void CellClick(int idx)
{
    if (_selectedCell == -1)
    {
        // Первый клик — выбираем ячейку (подсвечиваем золотым)
        _selectedCell = idx;
        _cellPanels[idx].BackColor = Color.Gold;
    }
    else if (_selectedCell == idx)
    {
        // Клик по той же ячейке — снимаем выбор
        _selectedCell = -1;
        _cellPanels[idx].BackColor = Color.LightGray;
    }
    else
    {
        // Второй клик на другую ячейку — меняем местами
        (_arrangement[_selectedCell], _arrangement[idx]) =
            (_arrangement[idx], _arrangement[_selectedCell]);
        _selectedCell = -1;
        UpdatePuzzleDisplay();
    }
}

// ── Проверка: пазл сложен верно? ──────────────────────────────────────────

private bool IsCaptchaSolved() =>
    _arrangement[0] == 0 && _arrangement[1] == 1 &&
    _arrangement[2] == 2 && _arrangement[3] == 3;
```

### Подключение к конструктору

В конструкторе формы `AuthForm()` вызовите `InitializeCaptcha()`:

```csharp
public AuthForm()
{
    InitializeComponent();
    InitializeCaptcha(); // ← добавьте эту строку
}
```

### Обработчик кнопки «Перемешать»

Двойной клик на кнопку `btnShuffle` в конструкторе добавит обработчик. Вставьте:

```csharp
private void btnShuffle_Click(object sender, EventArgs e)
{
    ShufflePuzzle();
}
```

### Сброс капчи при повторном открытии формы

Когда пользователь выходит из системы и форма снова показывается, капча должна сброситься. Переопределите метод:

```csharp
protected override void OnVisibleChanged(EventArgs e)
{
    base.OnVisibleChanged(e);
    if (Visible)
    {
        txtLogin.Clear();
        txtPassword.Clear();
        ShufflePuzzle();
    }
}
```

---

## 7. Форма авторизации — логика входа

Двойной клик на кнопку `btnEnter` в конструкторе. Вставьте полный обработчик:

```csharp
private void btnEnter_Click(object sender, EventArgs e)
{
    string login    = txtLogin.Text.Trim();
    string password = txtPassword.Text;

    // 1. Проверка обязательных полей
    if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
    {
        MessageBox.Show("Заполните поля Логин и Пароль.", "Предупреждение",
            MessageBoxButtons.OK, MessageBoxIcon.Warning);
        return;
    }

    // 2. Поиск пользователя в базе
    var user = DatabaseHelper.GetUser(login);
    if (user == null)
    {
        MessageBox.Show(
            "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные.",
            "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        ShufflePuzzle();
        return;
    }

    // 3. Проверка блокировки
    if (user.IsBlocked)
    {
        MessageBox.Show("Вы заблокированы. Обратитесь к администратору.",
            "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Error);
        return;
    }

    // 4. Проверка капчи и пароля
    bool captchaOk  = IsCaptchaSolved();
    bool passwordOk = DatabaseHelper.ValidatePassword(login, password);

    if (!captchaOk || !passwordOk)
    {
        // Увеличиваем счётчик неудачных попыток
        DatabaseHelper.IncrementFailedAttempts(login);
        var updated = DatabaseHelper.GetUser(login);

        if (updated?.IsBlocked == true)
        {
            MessageBox.Show("Вы заблокированы. Обратитесь к администратору.",
                "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else if (!captchaOk)
        {
            MessageBox.Show("Капча не пройдена. Сложите изображение правильно.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
        else
        {
            MessageBox.Show(
                "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные.",
                "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        ShufflePuzzle();
        return;
    }

    // 5. Успешный вход
    DatabaseHelper.ResetFailedAttempts(login);
    MessageBox.Show("Вы успешно авторизовались.", "Успех",
        MessageBoxButtons.OK, MessageBoxIcon.Information);

    Form next = user.Role == "Администратор"
        ? new AdminForm(login, this)
        : new UserForm(login, this);
    next.Show();
    this.Hide();
}
```

---

## 8. Форма администратора — интерфейс

Правой кнопкой на проект → **Добавить → Форма Windows** → имя `AdminForm.cs`.

### Размер формы

- `Size` → `800; 480`
- `Text` → `Рабочий стол администратора`
- `FormBorderStyle` → `FixedSingle`
- `MaximizeBox` → `False`
- `StartPosition` → `CenterScreen`

### Добавляемые элементы

#### Label — заголовок

| Свойство | Значение |
|----------|----------|
| `Name` | `lblTitle` |
| `Text` | `Рабочий стол администратора` |
| `Font` | Segoe UI, 11pt, Bold |
| `Size` | `660; 26` |
| `Location` | `10; 12` |

#### Button — «Выйти»

| Свойство | Значение |
|----------|----------|
| `Name` | `btnLogout` |
| `Text` | `Выйти` |
| `Size` | `90; 28` |
| `Location` | `680; 10` |
| `BackColor` | `IndianRed` |
| `ForeColor` | `White` |
| `FlatStyle` | `Flat` |

#### Label — заголовок таблицы

| Свойство | Значение |
|----------|----------|
| `Text` | `Список пользователей:` |
| `Font` | Segoe UI, 9pt, Bold |
| `Location` | `10; 48` |

#### DataGridView — таблица пользователей

| Свойство | Значение |
|----------|----------|
| `Name` | `dgvUsers` |
| `Size` | `760; 230` |
| `Location` | `10; 70` |
| `ReadOnly` | `True` |
| `SelectionMode` | `FullRowSelect` |
| `MultiSelect` | `False` |
| `AllowUserToAddRows` | `False` |
| `AllowUserToDeleteRows` | `False` |
| `RowHeadersVisible` | `False` |
| `AutoSizeColumnsMode` | `Fill` |

> **📸 Место для скриншота:** форма AdminForm в конструкторе

#### Label — заголовок секции редактирования

| Свойство | Значение |
|----------|----------|
| `Text` | `Добавление / редактирование пользователя:` |
| `Font` | Segoe UI, 9pt, Bold |
| `Location` | `10; 312` |

#### Label + TextBox — поле «Логин» (редактирование)

Добавьте Label (`Логин:`, Location `10; 342`) и TextBox:

| Свойство | Значение |
|----------|----------|
| `Name` | `txtEditLogin` |
| `Size` | `140; 26` |
| `Location` | `70; 339` |

#### Label + TextBox — поле «Пароль» (редактирование)

Добавьте Label (`Пароль:`, Location `225; 342`) и TextBox:

| Свойство | Значение |
|----------|----------|
| `Name` | `txtEditPassword` |
| `Size` | `140; 26` |
| `Location` | `295; 339` |
| `PasswordChar` | `*` |

#### Label + ComboBox — поле «Роль»

Добавьте Label (`Роль:`, Location `450; 342`) и ComboBox:

| Свойство | Значение |
|----------|----------|
| `Name` | `cmbEditRole` |
| `Size` | `150; 26` |
| `Location` | `500; 339` |
| `DropDownStyle` | `DropDownList` |
| `Items` | `Пользователь` и `Администратор` (через коллекцию Items в свойствах) |

#### CheckBox — «Заблокирован»

| Свойство | Значение |
|----------|----------|
| `Name` | `chkBlocked` |
| `Text` | `Заблокирован` |
| `Location` | `665; 341` |

#### Button — «Новый пользователь»

| Свойство | Значение |
|----------|----------|
| `Name` | `btnNew` |
| `Text` | `Новый пользователь` |
| `Size` | `180; 30` |
| `Location` | `10; 378` |

#### Button — «Сохранить»

| Свойство | Значение |
|----------|----------|
| `Name` | `btnSave` |
| `Text` | `Сохранить` |
| `Size` | `160; 30` |
| `Location` | `200; 378` |
| `BackColor` | `SteelBlue` |
| `ForeColor` | `White` |
| `FlatStyle` | `Flat` |

#### Label — строка статуса

| Свойство | Значение |
|----------|----------|
| `Name` | `lblStatus` |
| `Size` | `760; 22` |
| `Location` | `10; 422` |
| `ForeColor` | `ForestGreen` |
| `Font` | Segoe UI, 9pt, Italic |

---

## 9. Форма администратора — логика

Откройте `AdminForm.cs` (не Designer!). Замените содержимое:

```csharp
namespace ВашНеймспейс
{
    public partial class AdminForm : Form
    {
        private readonly string _adminLogin;
        private readonly Form   _loginForm;
        private int?            _editingId; // null = добавляем нового, число = редактируем

        public AdminForm(string adminLogin, Form loginForm)
        {
            _adminLogin = adminLogin;
            _loginForm  = loginForm;
            InitializeComponent();
            SetupGrid();
            LoadUsers();
            lblTitle.Text = $"Рабочий стол администратора — {adminLogin}";
        }

        // Настройка столбцов таблицы (вызывается один раз из конструктора)
        private void SetupGrid()
        {
            dgvUsers.Columns.Clear();
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colId",      HeaderText = "ID",            Width = 40  });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colLogin",   HeaderText = "Логин",         Width = 170 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colRole",    HeaderText = "Роль",          Width = 160 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colBlocked", HeaderText = "Заблокирован",  Width = 110 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn
                { Name = "colFailed",  HeaderText = "Попыток входа", Width = 110 });
            dgvUsers.Columns["colId"]!.Visible = false; // ID скрываем
        }

        // Загрузить/обновить список пользователей
        private void LoadUsers()
        {
            dgvUsers.Rows.Clear();
            foreach (var u in DatabaseHelper.GetAllUsers())
                dgvUsers.Rows.Add(u.Id, u.Login, u.Role,
                    u.IsBlocked ? "Да" : "Нет", u.FailedAttempts);
            lblStatus.Text = string.Empty;
        }

        // Клик по строке таблицы — заполнить форму редактирования
        private void dgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            var row = dgvUsers.SelectedRows[0];
            _editingId              = Convert.ToInt32(row.Cells["colId"].Value);
            txtEditLogin.Text       = row.Cells["colLogin"].Value?.ToString() ?? "";
            txtEditPassword.Text    = string.Empty;
            cmbEditRole.SelectedItem = row.Cells["colRole"].Value?.ToString();
            chkBlocked.Checked      = row.Cells["colBlocked"].Value?.ToString() == "Да";
        }

        // Кнопка «Новый пользователь» — очистить форму
        private void btnNew_Click(object sender, EventArgs e)
        {
            _editingId                = null;
            txtEditLogin.Text         = string.Empty;
            txtEditPassword.Text      = string.Empty;
            cmbEditRole.SelectedIndex = 0;
            chkBlocked.Checked        = false;
            dgvUsers.ClearSelection();
            lblStatus.Text            = string.Empty;
            txtEditLogin.Focus();
        }

        // Кнопка «Сохранить»
        private void btnSave_Click(object sender, EventArgs e)
        {
            string login    = txtEditLogin.Text.Trim();
            string password = txtEditPassword.Text;
            string role     = cmbEditRole.SelectedItem?.ToString() ?? "Пользователь";
            bool   blocked  = chkBlocked.Checked;

            if (string.IsNullOrEmpty(login))
            {
                MessageBox.Show("Введите логин.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_editingId == null) // ── Добавление нового ──
            {
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Введите пароль для нового пользователя.",
                        "Предупреждение", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (DatabaseHelper.LoginExists(login))
                {
                    MessageBox.Show($"Пользователь с логином «{login}» уже существует.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DatabaseHelper.AddUser(login, password, role);
                lblStatus.Text = $"Пользователь «{login}» добавлен.";
            }
            else // ── Редактирование существующего ──
            {
                var existing = DatabaseHelper.GetUser(login);
                if (existing != null && existing.Id != _editingId)
                {
                    MessageBox.Show($"Пользователь с логином «{login}» уже существует.",
                        "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DatabaseHelper.UpdateUser(
                    _editingId.Value,
                    login,
                    string.IsNullOrEmpty(password) ? null : password,
                    role,
                    blocked);
                lblStatus.Text = $"Данные пользователя «{login}» обновлены.";
            }

            LoadUsers();
            btnNew_Click(sender, e);
        }

        // Кнопка «Выйти»
        private void btnLogout_Click(object sender, EventArgs e)
        {
            _loginForm.Show();
            this.Close();
        }
    }
}
```

Не забудьте подключить обработчики в конструкторе **через конструктор формы**:
- двойной клик по `dgvUsers` → `SelectionChanged`
- двойной клик по `btnNew` → `btnNew_Click`
- двойной клик по `btnSave` → `btnSave_Click`
- двойной клик по `btnLogout` → `btnLogout_Click`

---

## 10. Форма пользователя

Правой кнопкой на проект → **Добавить → Форма Windows** → имя `UserForm.cs`.

### Размер формы

- `Size` → `500; 300`
- `Text` → `Рабочий стол`
- `FormBorderStyle` → `FixedSingle`
- `MaximizeBox` → `False`
- `StartPosition` → `CenterScreen`

### Добавляемые элементы

| Контрол | Name | Text | Location | Size | Дополнительно |
|---------|------|------|----------|------|---------------|
| Label | `lblTitle` | `Рабочий стол пользователя` | `0; 30` | `500; 45` | Font 16pt Bold, TextAlign MiddleCenter |
| Label | `lblWelcome` | `Добро пожаловать!` | `0; 100` | `500; 32` | Font 12pt, TextAlign MiddleCenter |
| Label | `lblRole` | `Роль: Пользователь` | `0; 145` | `500; 24` | Font Italic, ForeColor Gray, TextAlign MiddleCenter |
| Button | `btnLogout` | `Выйти` | `190; 210` | `120; 32` | BackColor IndianRed, ForeColor White, FlatStyle Flat |

### Код UserForm.cs

```csharp
namespace ВашНеймспейс
{
    public partial class UserForm : Form
    {
        private readonly Form _loginForm;

        public UserForm(string login, Form loginForm)
        {
            _loginForm = loginForm;
            InitializeComponent();
            lblWelcome.Text = $"Добро пожаловать, {login}!";
        }

        private void btnLogout_Click(object sender, EventArgs e)
        {
            _loginForm.Show();
            this.Close();
        }
    }
}
```

---

## 11. Точка входа Program.cs

Откройте `Program.cs` и добавьте вызов инициализации БД **перед** запуском приложения:

```csharp
namespace ВашНеймспейс
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            DatabaseHelper.Initialize(); // ← создаёт таблицу и тестовых пользователей
            ApplicationConfiguration.Initialize();
            Application.Run(new AuthForm());
        }
    }
}
```

---

## 12. Адаптация под свою базу данных

Если в вашем задании уже есть готовая база данных (SQL Server, MySQL, PostgreSQL и т.д.), класс `DatabaseHelper` нужно переписать, но **логика форм остаётся такой же**.

### Таблица «Пользователи»

Если таблица уже существует, добавьте в неё недостающие столбцы:

```sql
-- Пример для SQL Server / MySQL
ALTER TABLE Пользователи ADD IsBlocked    BIT           NOT NULL DEFAULT 0;
ALTER TABLE Пользователи ADD FailedAttempts INT          NOT NULL DEFAULT 0;
ALTER TABLE Пользователи ADD Role          NVARCHAR(50) NOT NULL DEFAULT N'Пользователь';
```

Если таблицы нет — создайте:

```sql
CREATE TABLE Пользователи (
    Id             INT IDENTITY PRIMARY KEY,  -- для MySQL: INT AUTO_INCREMENT
    Login          NVARCHAR(100) NOT NULL UNIQUE,
    Password       NVARCHAR(64)  NOT NULL,    -- SHA-256 в hex = 64 символа
    Role           NVARCHAR(50)  NOT NULL DEFAULT N'Пользователь',
    IsBlocked      BIT           NOT NULL DEFAULT 0,
    FailedAttempts INT           NOT NULL DEFAULT 0
);
```

### Смена провайдера в DatabaseHelper

Установите нужный NuGet пакет:

| СУБД | NuGet пакет |
|------|-------------|
| SQL Server | `Microsoft.Data.SqlClient` |
| MySQL | `MySql.Data` или `Pomelo.EntityFrameworkCore.MySql` |
| PostgreSQL | `Npgsql` |
| SQLite | `Microsoft.Data.Sqlite` ← уже установлен |

Замените строки подключения и класс соединения:

```csharp
// SQLite (текущий вариант)
using Microsoft.Data.Sqlite;
private static string ConnectionString => "Data Source=users.db";
using var conn = new SqliteConnection(ConnectionString);

// SQL Server
using Microsoft.Data.SqlClient;
private static string ConnectionString =>
    "Server=localhost;Database=MyDb;Trusted_Connection=True;";
using var conn = new SqlConnection(ConnectionString);

// MySQL
using MySql.Data.MySqlClient;
private static string ConnectionString =>
    "Server=localhost;Database=mydb;Uid=root;Pwd=password;";
using var conn = new MySqlConnection(ConnectionString);
```

Остальной код в `DatabaseHelper` (запросы, параметры `@param`) **одинаков** для всех трёх СУБД — менять его не нужно.

> **Важно:** строку подключения лучше вынести в файл конфигурации или константу, чтобы не менять код при смене сервера.

---

## 13. Проверка работы

Запустите проект (`F5`). Порядок проверки:

### Авторизация

| Действие | Ожидаемый результат |
|----------|---------------------|
| Нажать «Войти» с пустыми полями | Предупреждение «Заполните поля» |
| Ввести `admin / admin123`, не решив капчу | «Капча не пройдена» |
| Решить капчу, ввести неверный пароль 3 раза | «Вы заблокированы…» |
| Ввести `admin / admin123` + решить капчу | «Вы успешно авторизовались» → AdminForm |
| Ввести `user1 / user123` + решить капчу | «Вы успешно авторизовались» → UserForm |

### Капча

| Действие | Ожидаемый результат |
|----------|---------------------|
| Открыть форму | Фрагменты перемешаны |
| Кнопка «Перемешать» | Новое перемешивание |
| Кликнуть один фрагмент | Подсвечивается золотым |
| Кликнуть второй фрагмент | Два фрагмента меняются местами |
| Кликнуть выбранный снова | Выделение снимается |

### Панель администратора

| Действие | Ожидаемый результат |
|----------|---------------------|
| Клик по строке таблицы | Поля заполняются данными |
| «Новый пользователь» | Поля очищаются |
| Добавить пользователя с существующим логином | «Пользователь уже существует» |
| Снять чекбокс «Заблокирован» и сохранить | Пользователь разблокирован |

---

## Частые ошибки

**Капча не отображается / исключение при загрузке**
→ Проверьте, что у файлов `1.png`–`4.png` свойство «Копировать в выходной каталог» = «Копировать, если новее».

**`SqliteException: no such table`**
→ Убедитесь, что `DatabaseHelper.Initialize()` вызывается в `Program.cs` **до** `Application.Run(...)`.

**Конструктор формы показывает ошибку**
→ Все контролы в `Designer.cs` должны быть объявлены как `private ТипКонтрола имя = null!;` — поля класса, а не локальные переменные внутри `InitializeComponent`.

**Пазл всегда показывает правильный порядок**
→ Проверьте цикл `do { shuffle } while (IsCaptchaSolved())` — без него при случайном совпадении капча не будет перемешана.
