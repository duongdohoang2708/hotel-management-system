using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotelManagementSystem.ViewModels
{
    class SidebarViewModel
    {
        public string UserName { get; set; }
        public string Role { get; set; }

        public string AccountStaffName { get; set; }
        public string AccountStaffPosition { get; set; }

        public ObservableCollection<MenuItemModel> MenuItems { get; set; }
        public ICommand LogoutCommand { get; }
        public ICommand ExitCommand { get; }
        public ICommand AccountCommand { get; }
        public ICommand StaffCommand { get; }
        public ICommand FacilityCommand { get; }

        public event Action? HomeRequested;
        public event Action? RoomRequested;
        public event Action? LogoutRequested;
        public event Action? GuestRequested;
        public event Action? RoomManagementRequested;
        public event Action? ServiceRequested;
        public event Action? ServiceCategoryRequested;
        public event Action? OpenFacilityRequested;
        public event Action? RoomFacilityRequested;
        public event Action? OpenStaffRequested;
        public event Action? ExitRequested;
        public event Action? OpenAccountRequested;

        public SidebarViewModel(Account account)
        {
            Role = account.Role;
            UserName = account.Username;

            var db = new HotelManagementDbContext();
            var staff = db.Staff.FirstOrDefault(s => s.StaffId == account.StaffId);
            AccountStaffName = staff != null ? staff.FullName : "Ẩn danh";
            AccountStaffPosition = staff != null ? staff.Position : "Không xác định";

            LogoutCommand = new RelayCommand(_ => LogOut());
            ExitCommand = new RelayCommand(_ => Exit());
            AccountCommand = new RelayCommand(_ => OpenAccounts());
            StaffCommand = new RelayCommand(_ => OpenStaff());
            FacilityCommand = new RelayCommand(_ => OpenFacilities());
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
            OpenFacilityRequested?.Invoke();
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
            OpenStaffRequested?.Invoke();
        }

        private void OpenAccounts()
        {
            OpenAccountRequested.Invoke();
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
