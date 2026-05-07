namespace WinFormsAuth
{
    public partial class AdminForm : Form
    {
        private readonly string _adminLogin;
        private readonly Form   _loginForm;
        private int?            _editingId;

        public AdminForm(string adminLogin, Form loginForm)
        {
            _adminLogin = adminLogin;
            _loginForm  = loginForm;

            InitializeComponent();
            SetupGrid();
            LoadUsers();

            lblTitle.Text = $"Рабочий стол администратора — {adminLogin}";
        }

        // ── Grid setup ─────────────────────────────────────────────────────────

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

            dgvUsers.Columns["colId"]!.Visible = false;
        }

        // ── Data ───────────────────────────────────────────────────────────────

        private void LoadUsers()
        {
            dgvUsers.Rows.Clear();
            foreach (var u in DatabaseHelper.GetAllUsers())
                dgvUsers.Rows.Add(u.Id, u.Login, u.Role,
                    u.IsBlocked ? "Да" : "Нет", u.FailedAttempts);
            lblStatus.Text = string.Empty;
        }

        // ── Event handlers ─────────────────────────────────────────────────────

        private void dgvUsers_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvUsers.SelectedRows.Count == 0) return;
            var row = dgvUsers.SelectedRows[0];
            _editingId = Convert.ToInt32(row.Cells["colId"].Value);
            txtEditLogin.Text       = row.Cells["colLogin"].Value?.ToString() ?? "";
            txtEditPassword.Text    = string.Empty;
            cmbEditRole.SelectedItem = row.Cells["colRole"].Value?.ToString();
            chkBlocked.Checked      = row.Cells["colBlocked"].Value?.ToString() == "Да";
        }

        private void btnNew_Click(object sender, EventArgs e)
        {
            _editingId = null;
            txtEditLogin.Text    = string.Empty;
            txtEditPassword.Text = string.Empty;
            cmbEditRole.SelectedIndex = 0;
            chkBlocked.Checked        = false;
            dgvUsers.ClearSelection();
            lblStatus.Text = string.Empty;
            txtEditLogin.Focus();
        }

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

            if (_editingId == null) // ─── Add new user ───
            {
                if (string.IsNullOrEmpty(password))
                {
                    MessageBox.Show("Введите пароль для нового пользователя.", "Предупреждение",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                if (DatabaseHelper.LoginExists(login))
                {
                    MessageBox.Show($"Пользователь с логином «{login}» уже существует.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }
                DatabaseHelper.AddUser(login, password, role);
                lblStatus.Text = $"Пользователь «{login}» добавлен.";
            }
            else // ─── Edit existing user ───
            {
                var existing = DatabaseHelper.GetUser(login);
                if (existing != null && existing.Id != _editingId)
                {
                    MessageBox.Show($"Пользователь с логином «{login}» уже существует.", "Ошибка",
                        MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnLogout_Click(object sender, EventArgs e)
        {
            _loginForm.Show();
            this.Close();
        }
    }
}
