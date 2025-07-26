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
        public ICommand RoomTypeCommand { get; }
        public ICommand BookingStatusCommand { get; }
        public ICommand GuestCommand { get; }
        public ICommand ServiceCategoryCommand { get; }
        public ICommand ServiceCommand { get; }
        public ICommand RoomCommand { get; }
        public ICommand BookingCommand {get;}
        public ICommand NavigateCheckInCommand { get; }
        public ICommand NavigateCheckoutCommand { get; }
        public ICommand NavigateServiceUsageCommand { get; }
        public ICommand NavigateReportCommand { get; }
        public event Action? OpenCheckInRequested;
        public event Action? OpenCheckoutRequested;
        public event Action? OpenServiceUsageRequested;

        public event Action? HomeRequested;
        public event Action? OpenBookingRequested;
        public event Action? OpenRoomRequested;
        public event Action? LogoutRequested;
        public event Action? OpenGuestRequested;
        //public event Action? RoomManagementRequested;
        public event Action? ServiceRequested;
        public event Action? ServiceCategoryRequested;
        public event Action? OpenFacilityRequested;
        public event Action? RoomFacilityRequested;
        public event Action? OpenStaffRequested;
        public event Action? ExitRequested;
        public event Action? OpenAccountRequested;
        public event Action? OpenRoomTypeRequested;
        public event Action? OpenBookingStatusRequested;
        public event Action? OpenReportRequested;

        // Role-based visibility properties
        public bool CanAccessBooking => Role == "Manager" || Role == "Receptionist";
        public bool CanAccessCheckIn => Role == "Manager" || Role == "Receptionist";
        public bool CanAccessCheckout => Role == "Manager" || Role == "Receptionist";
        public bool CanAccessServiceUsage => Role == "Manager" || Role == "Receptionist";
        public bool CanAccessReport => Role == "Manager";
        public bool CanAccessGuest => Role == "Manager";
        public bool CanAccessFacility => Role == "Manager";
        public bool CanAccessRoomType => Role == "Manager";
        public bool CanAccessBookingStatus => Role == "Manager";
        public bool CanAccessRoom => Role == "Manager";
        public bool CanAccessServiceCategory => Role == "Manager";
        public bool CanAccessService => Role == "Manager";
        public bool CanAccessStaff => Role == "Manager";
        public bool CanAccessAccount => Role == "Admin";

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
            RoomTypeCommand = new RelayCommand(_ => OpenRoomTypes());
            BookingStatusCommand = new RelayCommand(_ => OpenBookingStatuses());
            GuestCommand = new RelayCommand(_ => OpenGuests());
            ServiceCategoryCommand = new RelayCommand(_ => OpenServiceCategories());
            ServiceCommand = new RelayCommand(_ => OpenServices());
            RoomCommand = new RelayCommand(_ => OpenRooms());
            BookingCommand = new RelayCommand(_ => OpenBookings());
            NavigateCheckInCommand = new RelayCommand(_ => OpenCheckIn());
            NavigateCheckoutCommand = new RelayCommand(_ => OpenCheckout());
            NavigateServiceUsageCommand = new RelayCommand(_ => OpenServiceUsage());
            NavigateReportCommand = new RelayCommand(_ => OpenReport());
        }

        private void Home()
        {
            HomeRequested?.Invoke();
        }

        private void OpenBookings()
        {
            OpenBookingRequested?.Invoke();
        }


        private void OpenRooms()
        {
            OpenRoomRequested?.Invoke();
        }

        private void OpenRoomTypes()
        {
            OpenRoomTypeRequested?.Invoke();
        }

        private void OpenBookingStatuses()
        {
            OpenBookingStatusRequested?.Invoke();
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
            OpenGuestRequested?.Invoke();
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

        private void OpenCheckIn()
        {
            OpenCheckInRequested?.Invoke();
        }

        private void OpenCheckout()
        {
            OpenCheckoutRequested?.Invoke();
        }

        private void OpenServiceUsage()
        {
            OpenServiceUsageRequested?.Invoke();
        }

        private void OpenReport()
        {
            OpenReportRequested?.Invoke();
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
