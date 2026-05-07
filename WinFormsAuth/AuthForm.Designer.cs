namespace WinFormsAuth
{
    partial class AuthForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                components?.Dispose();
                if (_pieces != null)
                    foreach (var b in _pieces) b.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        private void InitializeComponent()
        {
            lblTitle = new Label();
            lblLogin = new Label();
            txtLogin = new TextBox();
            lblPassword = new Label();
            txtPassword = new TextBox();
            lblHint = new Label();
            pnlCaptcha = new Panel();
            btnShuffle = new Button();
            btnEnter = new Button();
            SuspendLayout();
            // lblTitle
            lblTitle.Location = new Point(0, 14);
            lblTitle.Size = new Size(460, 40);
            lblTitle.Text = "Авторизация";
            lblTitle.Font = new Font("Segoe UI", 18F, FontStyle.Bold);
            lblTitle.TextAlign = ContentAlignment.MiddleCenter;
            // lblLogin
            lblLogin.Location = new Point(40, 80);
            lblLogin.Size = new Size(80, 23);
            lblLogin.Text = "Логин:";
            lblLogin.TextAlign = ContentAlignment.MiddleRight;
            // txtLogin
            txtLogin.Location = new Point(130, 77);
            txtLogin.Size = new Size(280, 26);
            txtLogin.Font = new Font("Segoe UI", 10F);
            // lblPassword
            lblPassword.Location = new Point(40, 120);
            lblPassword.Size = new Size(80, 23);
            lblPassword.Text = "Пароль:";
            lblPassword.TextAlign = ContentAlignment.MiddleRight;
            // txtPassword
            txtPassword.Location = new Point(130, 117);
            txtPassword.Size = new Size(280, 26);
            txtPassword.Font = new Font("Segoe UI", 10F);
            txtPassword.PasswordChar = '*';
            // lblHint
            lblHint.Location = new Point(40, 158);
            lblHint.Size = new Size(380, 22);
            lblHint.Text = "Сложите изображение (кликните две части, чтобы поменять местами):";
            lblHint.AutoSize = false;
            // pnlCaptcha  — cells are added programmatically in InitializeCaptcha()
            pnlCaptcha.Location = new Point(100, 185);
            pnlCaptcha.Size = new Size(260, 260);
            // btnShuffle
            btnShuffle.Location = new Point(130, 458);
            btnShuffle.Size = new Size(200, 28);
            btnShuffle.Text = "Перемешать";
            btnShuffle.UseVisualStyleBackColor = true;
            btnShuffle.Click += btnShuffle_Click;
            // btnEnter
            btnEnter.Location = new Point(130, 498);
            btnEnter.Size = new Size(200, 36);
            btnEnter.Text = "Войти";
            btnEnter.Font = new Font("Segoe UI", 11F, FontStyle.Bold);
            btnEnter.BackColor = Color.SteelBlue;
            btnEnter.ForeColor = Color.White;
            btnEnter.FlatStyle = FlatStyle.Flat;
            btnEnter.FlatAppearance.BorderSize = 0;
            btnEnter.Click += btnEnter_Click;
            // Form
            AutoScaleDimensions = new SizeF(7F, 15F);
            AutoScaleMode = AutoScaleMode.Font;
            ClientSize = new Size(460, 555);
            Text = "Авторизация";
            FormBorderStyle = FormBorderStyle.FixedSingle;
            MaximizeBox = false;
            StartPosition = FormStartPosition.CenterScreen;
            Controls.Add(lblTitle);
            Controls.Add(lblLogin);
            Controls.Add(txtLogin);
            Controls.Add(lblPassword);
            Controls.Add(txtPassword);
            Controls.Add(lblHint);
            Controls.Add(pnlCaptcha);
            Controls.Add(btnShuffle);
            Controls.Add(btnEnter);
            ResumeLayout(false);
        }

        #endregion

        private Label   lblTitle    = null!;
        private Label   lblLogin    = null!;
        private TextBox txtLogin    = null!;
        private Label   lblPassword = null!;
        private TextBox txtPassword = null!;
        private Label   lblHint     = null!;
        private Panel   pnlCaptcha  = null!;
        private Button  btnShuffle  = null!;
        private Button  btnEnter    = null!;
    }
}
