using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Views.UserControls;
using HotelManagementSystem.Views.Windows;
using HotelManagementSystem.Converters;
using HotelManagementSystem.Models;
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
using System.ComponentModel;

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
                    mainVm.CurrentViewTitle = "Quản lý Tài khoản";
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
                    mainVm.CurrentViewTitle = "Quản lý Nhân viên";
                }
            };

            //Đăng ký event chuyển sang view Quản lý Facility
            sideBarviewModel.OpenFacilityRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new FacilityManagementViewModel();
                    vm.RequestAddEditFacility += facility =>
                    {
                        var editVm = new FacilityAddEditViewModel(facility != null, facility);
                        var win = new FacilityAddEditWindow(editVm);
                        editVm.FacilitySaved += _ => vm.LoadFacilities();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new FacilityManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                    mainVm.CurrentViewTitle = "Quản lý Danh mục Thiết bị";
                }
            };

            //Đăng ký event chuyển sang view Quản lý Rooms
            sideBarviewModel.OpenRoomRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new RoomManagementViewModel();
                    vm.RequestAddEditRoom += room =>
                    {
                        var editVm = new RoomAddEditViewModel(room != null, room);
                        var win = new RoomAddEditWindow(editVm);
                        editVm.RoomSaved += _ => vm.LoadRooms();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    // Gán handler cho OnAddRoomFacilityRequested
                    vm.OnAddRoomFacilityRequested = (vmm) =>
                    {
                        System.Diagnostics.Debug.WriteLine("OnAddFacilityRequested callback triggered");
                        var winVm = new RoomFacilityAddEditViewModel(false, null, vmm.SelectedRoomDisplay);
                        var win = new RoomFacilityAddEditWindow(winVm);
                        winVm.RoomFacilitySaved += _ => vm.LoadRoomFacilities();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    vm.OnEditRoomFacilityRequested = (vmm, roomFacility) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"OnEditFacilityRequested callback triggered. roomFacility: {roomFacility.RoomFacilityId}");
                        var winVm = new RoomFacilityAddEditViewModel(true, roomFacility);
                        var win = new RoomFacilityAddEditWindow(winVm);
                        winVm.RoomFacilitySaved += _ => vm.LoadRoomFacilities();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new RoomManagementUserControl(vm);
                    mainVm.CurrentView = control;
                    mainVm.CurrentViewTitle = "Quản lý Phòng";
                }
            };

            //Đăng ký event chuyển sang view Quản lý RoomType
            sideBarviewModel.OpenRoomTypeRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new RoomTypeManagementViewModel();
                    vm.RequestAddEditRoomType += roomType =>
                    {
                        var editVm = new RoomTypeAddEditViewModel(roomType != null, roomType);
                        var win = new RoomTypeAddEditWindow(editVm);
                        editVm.RoomTypeSaved += _ => vm.LoadRoomTypes();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new RoomTypeManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                    mainVm.CurrentViewTitle = "Quản lý Loại phòng";
                }
            };

            //Đăng ký event chuyển sang view Quản lý BookingStatus
            sideBarviewModel.OpenBookingStatusRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new BookingStatusManagementViewModel();
                    vm.RequestAddEditBookingStatus += bookingStatus =>
                    {
                        var editVm = new BookingStatusAddEditViewModel(bookingStatus != null, bookingStatus);
                        var win = new BookingStatusAddEditWindow(editVm);
                        editVm.BookingStatusSaved += _ => vm.LoadBookingStatuses();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new BookingStatusManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                    mainVm.CurrentViewTitle = "Quản lý Trạng thái đặt phòng";
                }
            };

            //Đăng ký event chuyển sang view Quản lý Guest
            sideBarviewModel.OpenGuestRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new GuestManagementViewModel();
                    vm.RequestAddEditGuest += guest =>
                    {
                        var editVm = new GuestAddEditViewModel(guest != null, guest);
                        var win = new GuestAddEditWindow(editVm);
                        editVm.GuestSaved += _ => vm.LoadGuests();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new GuestManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                    mainVm.CurrentViewTitle = "Quản lý Khách hàng";
                }
            };

            //Đăng ký event chuyển sang view Quản lý ServiceCategory
            sideBarviewModel.ServiceCategoryRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new ServiceCategoryManagementViewModel();
                    vm.RequestAddEditServiceCategory += serviceCategory =>
                    {
                        var editVm = new ServiceCategoryAddEditViewModel(serviceCategory != null, serviceCategory);
                        var win = new ServiceCategoryAddEditWindow(editVm);
                        editVm.ServiceCategorySaved += _ => vm.LoadServiceCategories();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new ServiceCategoryManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                    mainVm.CurrentViewTitle = "Quản lý danh mục Loại dịch vụ";
                }
            };

            //Đăng ký event chuyển sang view Quản lý Service
            sideBarviewModel.ServiceRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    var vm = new ServiceManagementViewModel();
                    vm.RequestAddEditService += service =>
                    {
                        var editVm = new ServiceAddEditViewModel(service != null, service);
                        var win = new ServiceAddEditWindow(editVm);
                        editVm.ServiceSaved += _ => vm.LoadServices();
                        win.Owner = this;
                        win.ShowDialog();
                    };
                    var control = new ServiceManagementUserControl();
                    control.DataContext = vm;
                    mainVm.CurrentView = control;
                    mainVm.CurrentViewTitle = "Quản lý danh mục Dịch vụ";
                }
            };

            // Đăng ký event chuyển MH trang chủ
            sideBarviewModel.HomeRequested += () =>
            {
                if (this.DataContext is MainWindowViewModel mainVm)
                {
                    mainVm.CurrentView = new WelcomeUserControl();
                    mainVm.CurrentViewTitle = "Trang chủ";
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