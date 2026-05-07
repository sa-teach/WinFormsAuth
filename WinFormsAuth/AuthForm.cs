namespace WinFormsAuth
{
    public partial class AuthForm : Form
    {
        private Bitmap[]? _pieces;
        private readonly int[] _arrangement = [0, 1, 2, 3];
        private int _selectedCell = -1;

        private readonly Panel[]      _cellPanels   = new Panel[4];
        private readonly PictureBox[] _cellPictures = new PictureBox[4];

        public AuthForm()
        {
            InitializeComponent();
            InitializeCaptcha();
        }

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

        // ── Captcha ────────────────────────────────────────────────────────────

        private void InitializeCaptcha()
        {
            // Create puzzle cells and add to pnlCaptcha
            int[] xs = new int[] { 0, 130, 0,  130 };
            int[] ys = new int[] { 0, 0,  130, 130 };

            for (int i = 0; i < 4; i++)
            {
                int ci = i; // capture loop variable

                _cellPanels[i] = new Panel
                {
                    Location  = new Point(xs[i], ys[i]),
                    Size      = new Size(120, 120),
                    BackColor = Color.LightGray,
                    Cursor    = Cursors.Hand
                };

                _cellPictures[i] = new PictureBox
                {
                    Location = new Point(3, 3),
                    Size     = new Size(114, 114),
                    SizeMode = PictureBoxSizeMode.StretchImage,
                    Cursor   = Cursors.Hand
                };

                _cellPictures[i].Click += (_, _) => CellClick(ci);
                _cellPanels[i].Click   += (_, _) => CellClick(ci);
                _cellPanels[i].Controls.Add(_cellPictures[i]);
                pnlCaptcha.Controls.Add(_cellPanels[i]);
            }

            // Load 4 puzzle pieces from separate files: 1.png, 2.png, 3.png, 4.png
            string imgDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Img");
            _pieces = new Bitmap[4];
            for (int i = 0; i < 4; i++)
                _pieces[i] = new Bitmap(Path.Combine(imgDir, $"{i + 1}.png"));

            ShufflePuzzle();
        }

        private void ShufflePuzzle()
        {
            var rng = new Random();
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

        private void UpdatePuzzleDisplay()
        {
            if (_pieces == null) return;
            for (int i = 0; i < 4; i++)
            {
                _cellPictures[i].Image    = _pieces[_arrangement[i]];
                _cellPanels[i].BackColor  = (i == _selectedCell) ? Color.Gold : Color.LightGray;
            }
        }

        private void CellClick(int idx)
        {
            if (_selectedCell == -1)
            {
                _selectedCell = idx;
                _cellPanels[idx].BackColor = Color.Gold;
            }
            else if (_selectedCell == idx)
            {
                _selectedCell = -1;
                _cellPanels[idx].BackColor = Color.LightGray;
            }
            else
            {
                (_arrangement[_selectedCell], _arrangement[idx]) =
                    (_arrangement[idx], _arrangement[_selectedCell]);
                _selectedCell = -1;
                UpdatePuzzleDisplay();
            }
        }

        private bool IsCaptchaSolved() =>
            _arrangement[0] == 0 && _arrangement[1] == 1 &&
            _arrangement[2] == 2 && _arrangement[3] == 3;

        // ── Event handlers ─────────────────────────────────────────────────────

        private void btnShuffle_Click(object sender, EventArgs e) => ShufflePuzzle();

        private void btnEnter_Click(object sender, EventArgs e)
        {
            string login    = txtLogin.Text.Trim();
            string password = txtPassword.Text;

            if (string.IsNullOrEmpty(login) || string.IsNullOrEmpty(password))
            {
                MessageBox.Show("Заполните поля Логин и Пароль.", "Предупреждение",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var user = DatabaseHelper.GetUser(login);
            if (user == null)
            {
                MessageBox.Show(
                    "Вы ввели неверный логин или пароль. Пожалуйста проверьте ещё раз введенные данные.",
                    "Ошибка", MessageBoxButtons.OK, MessageBoxIcon.Error);
                ShufflePuzzle();
                return;
            }

            if (user.IsBlocked)
            {
                MessageBox.Show("Вы заблокированы. Обратитесь к администратору.",
                    "Доступ запрещён", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            bool captchaOk  = IsCaptchaSolved();
            bool passwordOk = DatabaseHelper.ValidatePassword(login, password);

            if (!captchaOk || !passwordOk)
            {
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

            DatabaseHelper.ResetFailedAttempts(login);
            MessageBox.Show("Вы успешно авторизовались.", "Успех",
                MessageBoxButtons.OK, MessageBoxIcon.Information);

            Form next = user.Role == "Администратор"
                ? new AdminForm(login, this)
                : new UserForm(login, this);
            next.Show();
            this.Hide();
        }
    }
}
