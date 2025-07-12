using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Views.UserControls;
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
        private bool isSidebarOpen = false;

        public MainWindow()
        {
            InitializeComponent();
            SidebarColumn.Width = new GridLength(0);
            Sidebar.Visibility = Visibility.Collapsed;

            // Gán DataContext cho MainWindow để binding hoạt động
            this.DataContext = new MainWindowViewModel();

            // Lấy role và userName từ tài khoản đăng nhập
            string role = AppSession.CurrentAccount?.Role ?? "Guest";
            string userName = AppSession.CurrentAccount?.Username ?? "";
            var sideBarviewModel = new SideBarUserControlViewModel(role, userName);
            Sidebar.DataContext = sideBarviewModel;

            // Đăng ký event chuyển MH trang chủ
            sideBarviewModel.HomeRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm) 
                {
                    mainVm.CurrentView = new WelcomeUserControl();
                }
            };

            // Đăng ký event chuyển MH trạng thái phòng
            sideBarviewModel.RoomRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    mainVm.CurrentView = new RoomStatusUserControl();
                }
            };

            // Đăng ký event chuyển MH QL phòng
            sideBarviewModel.RoomManagementRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    mainVm.CurrentView = new RoomManagementControl();
                }
            };

            // Đăng ký event logout
            sideBarviewModel.LogoutRequested += () =>
            {
                AppSession.CurrentAccount = null;
                this.Close();
            };

            // Đăng ký event chuyển MH QL khách hàng
            sideBarviewModel.CustomerRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    mainVm.CurrentView = new CustomerManagementUserControl();
                }
            };

            // Đăng ký event chuyển MH QL dịch vụ
            sideBarviewModel.ServiceRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var control = new ServiceManagementControl();
                    control.DataContext = new ServiceManagementViewModel();
                    mainVm.CurrentView = control;
                }
            };

            // Đăng ký event chuyển MH QL loại dịch vụ
            sideBarviewModel.ServiceCategoryRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var control = new ServiceCategoryManagementControl();
                    control.DataContext = new ServiceCategoryManagementViewModel();
                    mainVm.CurrentView = control;
                }
            };

            // Đăng ký event chuyển MH QL tiện nghi
            sideBarviewModel.AmenityRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var control = new AmenityManagementControl();
                    control.DataContext = new AmenityManagementViewModel();
                    mainVm.CurrentView = control;
                }
            };

            // Đăng ký event chuyển MH QL chi tiết tiện nghi
            sideBarviewModel.RoomAmenityRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var control = new RoomAmenityManagementControl();
                    control.DataContext = new RoomAmenityManagementViewModel();
                    mainVm.CurrentView = control;
                }
            };

            // Đăng ký event chuyển MH QL nhân viên
            sideBarviewModel.EmployeeRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var control = new EmployeeManagementControl();
                    control.DataContext = new EmployeeManagementViewModel();
                    mainVm.CurrentView = control;
                }
            };

            // Gán action mở chi tiết phòng mỗi khi CurrentView đổi
            if (this.DataContext is MainWindowViewModel mainVM)
            {
                SetRoomDetailAction(mainVM.CurrentView);
                // Lắng nghe thay đổi CurrentView nếu có implement INotifyPropertyChanged
                mainVM.PropertyChanged += (s, e) =>
                {
                    if (e.PropertyName == nameof(mainVM.CurrentView))
                    {
                        SetRoomDetailAction(mainVM.CurrentView);
                    }
                };
            }
        }

        private void SetRoomDetailAction(object? currentView)
        {
            if (currentView is HotelManagementSystem.Views.UserControls.RoomStatusUserControl uc &&
                uc.DataContext is HotelManagementSystem.ViewModels.RoomStatusViewModel vm)
            {
                vm.OpenRoomDetailAction = room =>
                {
                    var detailWindow = new HotelManagementSystem.Views.Windows.RoomDetailWindow();
                    var detailVm = new RoomDetailViewModel(room);
                    detailWindow.DataContext = detailVm;
                    // Lắng nghe event reload
                    detailWindow.RequestReloadRoomStatus += () =>
                    {
                        if (this.DataContext is MainWindowViewModel mainVm)
                        {
                            mainVm.CurrentView = new RoomStatusUserControl();
                        }
                    };
                    detailWindow.ShowDialog();
                };
            }
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