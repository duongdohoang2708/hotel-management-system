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
        public bool ShowBill { get; set; }
        public bool ShowCustomer { get; set; }
        public bool ShowRoom { get; set; }
        public bool ShowRoomType { get; set; }
        public bool ShowService { get; set; }
        public bool ShowServiceCategory { get; set; }
        public bool ShowAmenity { get; set; }
        public bool ShowRoomAmenity { get; set; }

        // Command cho từng nút
        public ICommand BillCommand { get; }
        public ICommand CustomerCommand { get; }
        public ICommand RoomCommand { get; }
        public ICommand RoomTypeCommand { get; }
        public ICommand ServiceCommand { get; }
        public ICommand ServiceCategoryCommand { get; }
        public ICommand AmenityCommand { get; }
        public ICommand RoomAmenityCommand { get; }
        public ICommand LogoutCommand { get; }

        public SideBarUserControlViewModel(string role, string userName = "")
        {
            Role = role;
            UserName = userName;

            // Phân quyền hiển thị các nút theo role
            ShowBill = role == "Admin" || role == "Manager";
            ShowCustomer = role == "Admin" || role == "Manager";
            ShowRoom = role == "Admin" || role == "Manager";
            ShowRoomType = role == "Admin";
            ShowService = role == "Admin" || role == "Manager";
            ShowServiceCategory = role == "Admin";
            ShowAmenity = role == "Admin";
            ShowRoomAmenity = role == "Admin";

            BillCommand = new RelayCommand(_ => { /* Xử lý chuyển trang hóa đơn */ });
            CustomerCommand = new RelayCommand(_ => { /* Xử lý chuyển trang khách hàng */ });
            RoomCommand = new RelayCommand(_ => { /* Xử lý chuyển trang phòng */ });
            RoomTypeCommand = new RelayCommand(_ => { /* Xử lý chuyển trang loại phòng */ });
            ServiceCommand = new RelayCommand(_ => { /* Xử lý chuyển trang dịch vụ */ });
            ServiceCategoryCommand = new RelayCommand(_ => { /* Xử lý chuyển trang loại dịch vụ */ });
            AmenityCommand = new RelayCommand(_ => { /* Xử lý chuyển trang tiện nghi */ });
            RoomAmenityCommand = new RelayCommand(_ => { /* Xử lý chuyển trang chi tiết tiện nghi */ });
            LogoutCommand = new RelayCommand(_ => { /* Xử lý đăng xuất */ });
        }
    }
}
