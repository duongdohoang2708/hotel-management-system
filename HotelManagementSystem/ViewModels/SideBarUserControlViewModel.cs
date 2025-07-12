using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotelManagementSystem.ViewModels
{
    public class SideBarUserControlViewModel : ViewModelBase
    {
        public string UserName { get; set; }
        public string Role { get; set; }

        // Các thuộc tính để ẩn/hiện menu
        public bool ShowHome { get; set; }
        public bool ShowRoom { get; set; }
        public bool ShowBooking { get; set; }
        public bool ShowBill { get; set; }
        public bool ShowCustomer { get; set; }
        public bool ShowRoomManagement { get; set; }
        public bool ShowService { get; set; }
        public bool ShowServiceCategory { get; set; }
        public bool ShowAmenity { get; set; }
        public bool ShowRoomAmenity { get; set; }
        public bool ShowAccount { get; set; }
        public bool ShowEmployee { get; set; }
        public bool ShowStatistic { get; set; }

        // Command cho từng nút
        public ICommand HomeCommand { get; }
        public ICommand RoomCommand { get; }
        public ICommand BookingCommand { get; }
        public ICommand BillCommand { get; }
        public ICommand CustomerCommand { get; }
        public ICommand RoomManagementCommand { get; }
        public ICommand ServiceCommand { get; }
        public ICommand ServiceCategoryCommand { get; }
        public ICommand AmenityCommand { get; }
        public ICommand RoomAmenityCommand { get; }
        public ICommand AccountCommand { get; }
        public ICommand EmployeeCommand { get; }
        public ICommand StatisticCommand { get; }
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

            // Phân quyền hợp lý cho 3 role: admin, manager, receptionist
            ShowHome = true;
            ShowBooking = role == "Manager" || role == "Receptionist";
            ShowRoom = role == "Manager" || role == "Receptionist";
            ShowRoomManagement = role == "Manager";
            ShowCustomer = role == "Manager" || role == "Receptionist";
            ShowBill = role == "Manager" || role == "Receptionist";
            ShowService = role == "Manager" || role == "Receptionist";
            ShowServiceCategory = role == "Manager";
            ShowAmenity = role == "Manager";
            ShowRoomAmenity = role == "Manager";
            ShowAccount = role == "Admin";
            ShowEmployee = role == "Admin" || role == "Manager";
            ShowStatistic = role == "Admin" || role == "Manager";

            HomeCommand = new RelayCommand(_ => Home());
            RoomCommand = new RelayCommand(_ => Room());
            BookingCommand = new RelayCommand(_ => { /* Xử lý chuyển trang Đặt Phòng */ });
            BillCommand = new RelayCommand(_ => { /* Xử lý chuyển trang Hóa đơn */ });
            CustomerCommand = new RelayCommand(_ => Customer());
            RoomManagementCommand = new RelayCommand(_ => RoomManagement());
            ServiceCommand = new RelayCommand(_ => Service());
            ServiceCategoryCommand = new RelayCommand(_ => ServiceCategory());
            AmenityCommand = new RelayCommand(_ => Amenity());
            RoomAmenityCommand = new RelayCommand(_ => RoomAmenity());
            AccountCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL tài khoản */ });
            EmployeeCommand = new RelayCommand(_ => Employee());
            StatisticCommand = new RelayCommand(_ => { /* Xử lý chuyển trang Thống kê */ });
            LogoutCommand = new RelayCommand(_ => LogOut());
        }

        private void Home()
        {
            HomeRequested?.Invoke();
        }

        private void Room()
        {
            RoomRequested.Invoke();
        }

        private void Customer()
        {
            CustomerRequested?.Invoke();
        }

        private void RoomManagement()
        {
            RoomManagementRequested?.Invoke();
        }

        private void Service()
        {
            ServiceRequested?.Invoke();
        }

        private void ServiceCategory()
        {
            ServiceCategoryRequested?.Invoke();
        }

        private void Amenity()
        {
            AmenityRequested?.Invoke();
        }

        private void RoomAmenity()
        {
            RoomAmenityRequested?.Invoke();
        }

        private void Employee()
        {
            EmployeeRequested?.Invoke();
        }

        private void LogOut()
        {
            LogoutRequested?.Invoke();
        }
    }
}
