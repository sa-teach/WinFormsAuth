namespace WinFormsAuth
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
