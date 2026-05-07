namespace WinFormsAuth
{
    internal static class Program
    {
        [STAThread]
        static void Main()
        {
            DatabaseHelper.Initialize();
            ApplicationConfiguration.Initialize();
            Application.Run(new AuthForm());
        }
    }
}
