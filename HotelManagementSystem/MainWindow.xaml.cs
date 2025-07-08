using HotelManagementSystem.ViewModels;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace HotelManagementSystem
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private bool isSidebarOpen = true;

        public MainWindow()
        {
            InitializeComponent();
            SidebarColumn.Width = new GridLength(0);
            Sidebar.Visibility = Visibility.Collapsed;
            isSidebarOpen = false;

            // Lấy role và userName từ tài khoản đăng nhập
            string role = AppSession.CurrentAccount?.Role ?? "Guest";
            string userName = AppSession.CurrentAccount?.Username ?? "";
            Sidebar.DataContext = new SideBarUserControlViewModel(role, userName);
        }

        private void HamburgerButton_Click(object sender, RoutedEventArgs e)
        {
            if (isSidebarOpen)
            {
                SidebarColumn.Width = new GridLength(0);
                Sidebar.Visibility = Visibility.Collapsed;
            }
            else
            {
                SidebarColumn.Width = new GridLength(220);
                Sidebar.Visibility = Visibility.Visible;
            }
            isSidebarOpen = !isSidebarOpen;
        }
    }
}