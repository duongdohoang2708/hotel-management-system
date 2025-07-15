using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using HotelManagementSystem.Models;
using HotelManagementSystem;

namespace HotelManagementSystem.ViewModels
{
    public class SideBarUserControlViewModel : ViewModelBase
    {
        public string UserName { get; set; }
        public string Role { get; set; }

        public string StaffName{ get; set; }

        public ObservableCollection<MenuItemModel> MenuItems { get; set; }
        public ICommand LogoutCommand { get; }

        public event Action? HomeRequested;
        public event Action? RoomRequested;
        public event Action? LogoutRequested;
        public event Action? GuestRequested;
        public event Action? RoomManagementRequested;
        public event Action? ServiceRequested;
        public event Action? ServiceCategoryRequested;
        public event Action? FacilityRequested;
        public event Action? RoomFacilityRequested;
        public event Action? StaffRequested;
        public event Action? ExitRequested;

        public SideBarUserControlViewModel(Account account)
        {
            Role = account.Role;
            UserName = account.Username;

            var db = new HotelManagementDbContext();
            var staff = db.Staff.FirstOrDefault(s => s.StaffId == account.StaffId);
            StaffName = staff != null ? staff.FullName : "Người dùng ẩn danh";

            LogoutCommand = new RelayCommand(_ => LogOut());
            MenuItems = new ObservableCollection<MenuItemModel>
            {
                new MenuItemModel
                {
                    Title = "Trang chủ",
                    Icon = "🏠",
                    Command = new RelayCommand(_ => Home())
                },
                new MenuItemModel
                {
                    Title = "Quản lý phòng",
                    Icon = "🛏",
                    Children = new ObservableCollection<MenuItemModel>
                    {
                        new MenuItemModel { Title = "Danh sách phòng", Command = new RelayCommand(_ => OpenRooms()) },
                        new MenuItemModel { Title = "Loại phòng", Command = new RelayCommand(_ => OpenRoomTypes()) },
                        new MenuItemModel { Title = "Tiện nghi", Command = new RelayCommand(_ => OpenFacilities()) }
                    }
                },
                new MenuItemModel
                {
                    Title = "Đặt phòng",
                    Icon = "📅",
                    Command = new RelayCommand(_ => OpenReservations())
                },
                new MenuItemModel
                {
                    Title = "Dịch vụ",
                    Icon = "🧴",
                    Children = new ObservableCollection<MenuItemModel>
                    {
                        new MenuItemModel { Title = "Danh sách dịch vụ", Command = new RelayCommand(_ => OpenServices()) },
                        new MenuItemModel { Title = "Loại dịch vụ", Command = new RelayCommand(_ => OpenServiceCategories()) }
                    }
                },
                new MenuItemModel
                {
                    Title = "Khách hàng & Nhân viên",
                    Icon = "👥",
                    Children = new ObservableCollection<MenuItemModel>
                    {
                        new MenuItemModel { Title = "Khách hàng", Command = new RelayCommand(_ => OpenGuests()) },
                        new MenuItemModel { Title = "Nhân viên", Command = new RelayCommand(_ => OpenStaff()) },
                        new MenuItemModel { Title = "Tài khoản", Command = new RelayCommand(_ => OpenAccounts()) }
                    }
                },
                new MenuItemModel
                {
                    Title = "Hóa đơn",
                    Icon = "💵",
                    Command = new RelayCommand(_ => OpenBills())
                },
                new MenuItemModel
                {
                    Title = "Thống kê",
                    Icon = "📊",
                    Command = new RelayCommand(_ => OpenReports())
                },
                new MenuItemModel
                {
                    Title = "Thoát",
                    Icon = "🚪",
                    Command = new RelayCommand(_ => Exit())
                }
            };
        }

        private void Home()
        {
            HomeRequested?.Invoke();
        }

        private void OpenRooms()
        {
            RoomRequested?.Invoke();
        }

        private void OpenRoomTypes()
        {
            RoomManagementRequested?.Invoke();
        }

        private void OpenFacilities()
        {
            FacilityRequested?.Invoke();
        }

        private void OpenReservations()
        {
            // This command is not directly mapped to a specific action in the new menu structure
            // It might need to be handled differently if it triggers a navigation
        }

        private void OpenServices()
        {
            ServiceRequested?.Invoke();
        }

        private void OpenServiceCategories()
        {
            ServiceCategoryRequested?.Invoke();
        }

        private void OpenGuests()
        {
            GuestRequested?.Invoke();
        }

        private void OpenStaff()
        {
            StaffRequested?.Invoke();
        }

        private void OpenAccounts()
        {
            // This command is not directly mapped to a specific action in the new menu structure
            // It might need to be handled differently if it triggers a navigation
        }

        private void OpenBills()
        {
            // This command is not directly mapped to a specific action in the new menu structure
            // It might need to be handled differently if it triggers a navigation
        }

        private void OpenReports()
        {
            // This command is not directly mapped to a specific action in the new menu structure
            // It might need to be handled differently if it triggers a navigation
        }

        private void LogOut()
        {
            LogoutRequested?.Invoke();
        }

        private void Exit()
        {
            ExitRequested?.Invoke();
        }
    }
}
