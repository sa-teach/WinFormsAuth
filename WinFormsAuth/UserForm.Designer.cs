namespace WinFormsAuth
{
    partial class UserForm
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
            lblTitle   = new Label();
            lblWelcome = new Label();
            lblRole    = new Label();
            btnLogout  = new Button();

            SuspendLayout();

            // lblTitle
            lblTitle.Location  = new System.Drawing.Point(0, 30);
            lblTitle.Size      = new System.Drawing.Size(500, 45);
            lblTitle.Text      = "Рабочий стол пользователя";
            lblTitle.Font      = new System.Drawing.Font("Segoe UI", 16f, System.Drawing.FontStyle.Bold);
            lblTitle.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblTitle.Name      = "lblTitle";

            // lblWelcome
            lblWelcome.Location  = new System.Drawing.Point(0, 100);
            lblWelcome.Size      = new System.Drawing.Size(500, 32);
            lblWelcome.Font      = new System.Drawing.Font("Segoe UI", 12f);
            lblWelcome.Text      = "Добро пожаловать!";
            lblWelcome.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblWelcome.Name      = "lblWelcome";

            // lblRole
            lblRole.Location  = new System.Drawing.Point(0, 145);
            lblRole.Size      = new System.Drawing.Size(500, 24);
            lblRole.Font      = new System.Drawing.Font("Segoe UI", 9.5f, System.Drawing.FontStyle.Italic);
            lblRole.Text      = "Роль: Пользователь";
            lblRole.ForeColor = System.Drawing.Color.Gray;
            lblRole.TextAlign = System.Drawing.ContentAlignment.MiddleCenter;
            lblRole.Name      = "lblRole";

            // btnLogout
            btnLogout.Location  = new System.Drawing.Point(190, 210);
            btnLogout.Size      = new System.Drawing.Size(120, 32);
            btnLogout.Text      = "Выйти";
            btnLogout.BackColor = System.Drawing.Color.IndianRed;
            btnLogout.ForeColor = System.Drawing.Color.White;
            btnLogout.FlatStyle = FlatStyle.Flat;
            btnLogout.FlatAppearance.BorderSize = 0;
            btnLogout.Name      = "btnLogout";
            btnLogout.Click    += new System.EventHandler(btnLogout_Click);

            // Form
            AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            AutoScaleMode       = AutoScaleMode.Font;
            ClientSize          = new System.Drawing.Size(500, 300);
            Text                = "Рабочий стол";
            FormBorderStyle     = FormBorderStyle.FixedSingle;
            MaximizeBox         = false;
            StartPosition       = FormStartPosition.CenterScreen;
            Name                = "UserForm";

            Controls.Add(lblTitle);
            Controls.Add(lblWelcome);
            Controls.Add(lblRole);
            Controls.Add(btnLogout);

            ResumeLayout(false);
        }

        #endregion

        private Label  lblTitle   = null!;
        private Label  lblWelcome = null!;
        private Label  lblRole    = null!;
        private Button btnLogout  = null!;
    }
}
