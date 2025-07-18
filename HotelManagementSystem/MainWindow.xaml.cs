using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Views.UserControls;
using HotelManagementSystem.Views.Windows;
using HotelManagementSystem.Converters;
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
            SidebarColumn.Width = new GridLength(270);
            Sidebar.Visibility = Visibility.Visible;
            WindowState = WindowState.Maximized;

            // Gán DataContext cho MainWindow để binding hoạt động
            this.DataContext = new MainWindowViewModel();

            // Lấy role và userName từ tài khoản đăng nhập
            string role = AppSession.CurrentAccount?.Role ?? "Guest";
            string userName = AppSession.CurrentAccount?.Username ?? "";
            var sideBarviewModel = new SidebarViewModel(AppSession.CurrentAccount);

            Sidebar.DataContext = sideBarviewModel;

            //Đăng ký event chuyển sang view Quản lý Account
            sideBarviewModel.OpenAccountRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new AccountManagementViewModel();
                    vm.RequestAddEditAccount += account =>
                    {
                        var editVm = new AccountAddEditViewModel(account != null, account);
                        var win = new AccountAddEditWindow(editVm);
                        editVm.AccountSaved += _ => vm.LoadAccounts();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new AccountManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                }
            };

            //Đăng ký event chuyển sang view Quản lý Staff
            sideBarviewModel.OpenStaffRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new StaffManagementViewModel();
                    vm.RequestAddEditStaff += staff =>
                    {
                        var editVm = new StaffAddEditViewModel(staff != null, staff);
                        var win = new StaffAddEditWindow(editVm);
                        editVm.StaffSaved += _ => vm.LoadStaff();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new StaffManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                }
            };

            //Đăng ký event chuyển sang view Quản lý Facility
            sideBarviewModel.OpenFacilityRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new FacilitiesManagementViewModel();
                    vm.RequestAddEditFacility += facility =>
                    {
                        var editVm = new FacilityAddEditViewModel(facility != null, facility);
                        var win = new FacilityAddEditWindow(editVm);
                        editVm.FacilitySaved += _ => vm.LoadFacilities();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new StaffManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                }
            };

            // Đăng ký event chuyển MH trang chủ
            sideBarviewModel.HomeRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    mainVm.CurrentView = new WelcomeUserControl();
                }
            };


            // Đăng ký event logout
            sideBarviewModel.LogoutRequested += () =>
            {
                AppSession.CurrentAccount = null;
                this.Close();
                var loginWindow = new HotelManagementSystem.Views.Windows.LoginWindow();
                var result = loginWindow.ShowDialog();
                if (result == true && AppSession.CurrentAccount != null)
                {
                    var mainWindow = new MainWindow();
                    App.Current.MainWindow = mainWindow;
                    mainWindow.ShowDialog();
                }
            };

            //Đăng ký event Exit
            sideBarviewModel.ExitRequested += () => {
                var result = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn thoát ứng dụng?", "Xác nhận thoát", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
                if (result == System.Windows.MessageBoxResult.Yes)
                    this.Close();
            };

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
                SidebarColumn.Width = new GridLength(270);
                Sidebar.Visibility = Visibility.Visible;
            }
            isSidebarOpen = !isSidebarOpen;
        }
    }
}