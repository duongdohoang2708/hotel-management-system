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
        public bool ShowRoomType { get; set; }
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
        public ICommand RoomTypeCommand { get; }
        public ICommand ServiceCommand { get; }
        public ICommand ServiceCategoryCommand { get; }
        public ICommand AmenityCommand { get; }
        public ICommand RoomAmenityCommand { get; }
        public ICommand AccountCommand { get; }
        public ICommand EmployeeCommand { get; }
        public ICommand StatisticCommand { get; }
        public ICommand LogoutCommand { get; }
        
        public event Action? LogoutRequested;

        public SideBarUserControlViewModel(string role, string userName = "")
        {
            Role = role;
            UserName = userName;

            // Phân quyền hợp lý cho 3 role: admin, manager, receptionist
            ShowHome = true;
            ShowBooking = role == "Manager" || role == "Receptionist";
            ShowRoom = role == "Manager" || role == "Receptionist";
            ShowRoomManagement = role == "Manager";
            ShowRoomType = role == "Manager";
            ShowCustomer = role == "Manager" || role == "Receptionist";
            ShowBill = role == "Manager" || role == "Receptionist";
            ShowService = role == "Manager" || role == "Receptionist";
            ShowServiceCategory = role == "Manager";
            ShowAmenity = role == "Manager";
            ShowRoomAmenity = role == "Manager";
            ShowAccount = role == "Admin";
            ShowEmployee = role == "Admin" || role == "Manager";
            ShowStatistic = role == "Admin" || role == "Manager";

            HomeCommand = new RelayCommand(_ => { /* Xử lý chuyển trang Trang chủ */ });
            RoomCommand = new RelayCommand(_ => { /* Xử lý chuyển trang Phòng */ });
            BookingCommand = new RelayCommand(_ => { /* Xử lý chuyển trang Đặt Phòng */ });
            BillCommand = new RelayCommand(_ => { /* Xử lý chuyển trang Hóa đơn */ });
            CustomerCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL khách hàng */ });
            RoomManagementCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL phòng */ });
            RoomTypeCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL loại phòng */ });
            ServiceCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL dịch vụ */ });
            ServiceCategoryCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL loại dịch vụ */ });
            AmenityCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL tiện nghi */ });
            RoomAmenityCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL chi tiết tiện nghi */ });
            AccountCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL tài khoản */ });
            EmployeeCommand = new RelayCommand(_ => { /* Xử lý chuyển trang QL nhân viên */ });
            StatisticCommand = new RelayCommand(_ => { /* Xử lý chuyển trang Thống kê */ });
            LogoutCommand = new RelayCommand(_ => LogOut());
        }

        private void LogOut()
        {
            LogoutRequested?.Invoke();
        }
    }
}
