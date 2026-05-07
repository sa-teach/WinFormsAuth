namespace WinFormsAuth
{
    partial class AdminForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing) components?.Dispose();
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitle        = new Label();
            btnLogout       = new Button();
            lblUsersHdr     = new Label();
            dgvUsers        = new DataGridView();
            lblEditHdr      = new Label();
            lblLoginLbl     = new Label();
            txtEditLogin    = new TextBox();
            lblPassLbl      = new Label();
            txtEditPassword = new TextBox();
            lblRoleLbl      = new Label();
            cmbEditRole     = new ComboBox();
            chkBlocked      = new CheckBox();
            btnNew          = new Button();
            btnSave         = new Button();
            lblStatus       = new Label();

            ((System.ComponentModel.ISupportInitialize)dgvUsers).BeginInit();
            SuspendLayout();

            // lblTitle
            lblTitle.Location  = new System.Drawing.Point(10, 12);
            lblTitle.Size      = new System.Drawing.Size(660, 26);
            lblTitle.Font      = new System.Drawing.Font("Segoe UI", 11f, System.Drawing.FontStyle.Bold);
            lblTitle.Text      = "Рабочий стол администратора";
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
            lblTitle.Name      = "lblTitle";

            // btnLogout
            btnLogout.Location  = new System.Drawing.Point(680, 10);
            btnLogout.Size      = new System.Drawing.Size(90, 28);
            btnLogout.Text      = "Выйти";
            btnLogout.BackColor = System.Drawing.Color.IndianRed;
            btnLogout.ForeColor = System.Drawing.Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Name      = "btnLogout";
            btnLogout.Click    += new System.EventHandler(btnLogout_Click);

            // lblUsersHdr
            lblUsersHdr.Location = new System.Drawing.Point(10, 48);
            lblUsersHdr.Size     = new System.Drawing.Size(300, 20);
            lblUsersHdr.Text     = "Список пользователей:";
            lblUsersHdr.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            lblUsersHdr.Name     = "lblUsersHdr";

            // dgvUsers
            dgvUsers.Location                  = new System.Drawing.Point(10, 70);
            dgvUsers.Size                      = new System.Drawing.Size(760, 230);
            dgvUsers.AllowUserToAddRows        = false;
            dgvUsers.AllowUserToDeleteRows     = false;
            dgvUsers.ReadOnly                  = true;
            dgvUsers.SelectionMode             = DataGridViewSelectionMode.FullRowSelect;
            dgvUsers.MultiSelect               = false;
            dgvUsers.RowHeadersVisible         = false;
            dgvUsers.AutoSizeColumnsMode       = DataGridViewAutoSizeColumnsMode.Fill;
            dgvUsers.BackgroundColor           = System.Drawing.SystemColors.Window;
            dgvUsers.BorderStyle               = BorderStyle.Fixed3D;
            dgvUsers.Name                      = "dgvUsers";
            dgvUsers.SelectionChanged         += new System.EventHandler(dgvUsers_SelectionChanged);

            // lblEditHdr
            lblEditHdr.Location = new System.Drawing.Point(10, 312);
            lblEditHdr.Size     = new System.Drawing.Size(500, 20);
            lblEditHdr.Text     = "Добавление / редактирование пользователя:";
            lblEditHdr.Font     = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Bold);
            lblEditHdr.Name     = "lblEditHdr";

            // lblLoginLbl
            lblLoginLbl.Location  = new System.Drawing.Point(10, 342);
            lblLoginLbl.Size      = new System.Drawing.Size(55, 23);
            lblLoginLbl.Text      = "Логин:";
            lblLoginLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblLoginLbl.Name      = "lblLoginLbl";

            // txtEditLogin
            txtEditLogin.Location = new System.Drawing.Point(70, 339);
            txtEditLogin.Size     = new System.Drawing.Size(140, 26);
            txtEditLogin.Font     = new System.Drawing.Font("Segoe UI", 9.5f);
            txtEditLogin.Name     = "txtEditLogin";

            // lblPassLbl
            lblPassLbl.Location  = new System.Drawing.Point(225, 342);
            lblPassLbl.Size      = new System.Drawing.Size(65, 23);
            lblPassLbl.Text      = "Пароль:";
            lblPassLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblPassLbl.Name      = "lblPassLbl";

            // txtEditPassword
            txtEditPassword.Location     = new System.Drawing.Point(295, 339);
            txtEditPassword.Size         = new System.Drawing.Size(140, 26);
            txtEditPassword.Font         = new System.Drawing.Font("Segoe UI", 9.5f);
            txtEditPassword.PasswordChar = '*';
            txtEditPassword.Name         = "txtEditPassword";

            // lblRoleLbl
            lblRoleLbl.Location  = new System.Drawing.Point(450, 342);
            lblRoleLbl.Size      = new System.Drawing.Size(45, 23);
            lblRoleLbl.Text      = "Роль:";
            lblRoleLbl.TextAlign = System.Drawing.ContentAlignment.MiddleRight;
            lblRoleLbl.Name      = "lblRoleLbl";

            // cmbEditRole
            cmbEditRole.Location      = new System.Drawing.Point(500, 339);
            cmbEditRole.Size          = new System.Drawing.Size(150, 26);
            cmbEditRole.DropDownStyle = ComboBoxStyle.DropDownList;
            cmbEditRole.Items.AddRange(new object[] { "Пользователь", "Администратор" });
            cmbEditRole.SelectedIndex = 0;
            cmbEditRole.Name          = "cmbEditRole";

            // chkBlocked
            chkBlocked.Location = new System.Drawing.Point(665, 341);
            chkBlocked.Size     = new System.Drawing.Size(105, 23);
            chkBlocked.Text     = "Заблокирован";
            chkBlocked.Name     = "chkBlocked";

            // btnNew
            btnNew.Location              = new System.Drawing.Point(10, 378);
            btnNew.Size                  = new System.Drawing.Size(180, 30);
            btnNew.Text                  = "Новый пользователь";
            btnNew.UseVisualStyleBackColor = true;
            btnNew.Name                  = "btnNew";
            btnNew.Click                += new System.EventHandler(btnNew_Click);

            // btnSave
            btnSave.Location  = new System.Drawing.Point(200, 378);
            btnSave.Size      = new System.Drawing.Size(160, 30);
            btnSave.Text      = "Сохранить";
            btnSave.BackColor = System.Drawing.Color.SteelBlue;
            btnSave.ForeColor = System.Drawing.Color.White;
            btnSave.FlatStyle = FlatStyle.Flat;
            btnSave.FlatAppearance.BorderSize = 0;
            btnSave.Name      = "btnSave";
            btnSave.Click    += new System.EventHandler(btnSave_Click);

            // lblStatus
            lblStatus.Location  = new System.Drawing.Point(10, 422);
            lblStatus.Size      = new System.Drawing.Size(760, 22);
            lblStatus.ForeColor = System.Drawing.Color.ForestGreen;
            lblStatus.Font      = new System.Drawing.Font("Segoe UI", 9f, System.Drawing.FontStyle.Italic);
            lblStatus.Name      = "lblStatus";

            // Form
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(784, 455);
            Text                = "Рабочий стол администратора";
            FormBorderStyle     = FormBorderStyle.FixedSingle;
            MaximizeBox         = false;
            StartPosition       = FormStartPosition.CenterScreen;
            Name                = "AdminForm";

            Controls.Add(lblTitle);
            Controls.Add(btnLogout);
            Controls.Add(lblUsersHdr);
            Controls.Add(dgvUsers);
            Controls.Add(lblEditHdr);
            Controls.Add(lblLoginLbl);
            Controls.Add(txtEditLogin);
            Controls.Add(lblPassLbl);
            Controls.Add(txtEditPassword);
            Controls.Add(lblRoleLbl);
            Controls.Add(cmbEditRole);
            Controls.Add(chkBlocked);
            Controls.Add(btnNew);
            Controls.Add(btnSave);
            Controls.Add(lblStatus);

            ((System.ComponentModel.ISupportInitialize)dgvUsers).EndInit();
            ResumeLayout(false);
        }

        #endregion

        private Label        lblTitle        = null!;
        private Button       btnLogout       = null!;
        private Label        lblUsersHdr     = null!;
        private DataGridView dgvUsers        = null!;
        private Label        lblEditHdr      = null!;
        private Label        lblLoginLbl     = null!;
        private TextBox      txtEditLogin    = null!;
        private Label        lblPassLbl      = null!;
        private TextBox      txtEditPassword = null!;
        private Label        lblRoleLbl      = null!;
        private ComboBox     cmbEditRole     = null!;
        private CheckBox     chkBlocked      = null!;
        private Button       btnNew          = null!;
        private Button       btnSave         = null!;
        private Label        lblStatus       = null!;
    }
}
