using System.Configuration;
using System.Data;
using System.Windows;

namespace HotelManagementSystem
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            var loginWindow = new Views.Windows.LoginWindow();
            var result = loginWindow.ShowDialog();
            if (result == true && AppSession.CurrentAccount != null)
            {
                var mainWindow = new MainWindow();
                MainWindow = mainWindow;
                mainWindow.ShowDialog();
            }
            Shutdown();
        }
    }
}
