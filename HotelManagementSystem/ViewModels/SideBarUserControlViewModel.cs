using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class SideBarUserControlViewModel : ViewModelBase
    {
        public string UserName { get; set; }
        public string Role { get; set; }
        public ObservableCollection<MenuItemModel> MenuItems { get; set; }
        public ICommand LogoutCommand { get; }

        public event Action? HomeRequested;
        public event Action? RoomRequested;
        public event Action? LogoutRequested;
        public event Action? CustomerRequested;
        public event Action? RoomManagementRequested;
        public event Action? ServiceRequested;
        public event Action? ServiceCategoryRequested;
        public event Action? AmenityRequested;
        public event Action? RoomAmenityRequested;
        public event Action? EmployeeRequested;

        public SideBarUserControlViewModel(string role, string userName = "")
        {
            Role = role;
            UserName = userName;
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
                        new MenuItemModel { Title = "Tiện nghi", Command = new RelayCommand(_ => OpenAmenities()) }
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
                        new MenuItemModel { Title = "Khách hàng", Command = new RelayCommand(_ => OpenCustomers()) },
                        new MenuItemModel { Title = "Nhân viên", Command = new RelayCommand(_ => OpenEmployees()) },
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
                    Title = "Đăng xuất",
                    Icon = "🚪",
                    Command = LogoutCommand
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

        private void OpenAmenities()
        {
            AmenityRequested?.Invoke();
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

        private void OpenCustomers()
        {
            CustomerRequested?.Invoke();
        }

        private void OpenEmployees()
        {
            EmployeeRequested?.Invoke();
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
    }
}
