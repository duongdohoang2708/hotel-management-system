using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class BookingManagementViewModel : ViewModelBase
    {
        // Điều kiện tìm kiếm
        public DateTime? CheckInDate { get; set; }
        public DateTime? CheckOutDate { get; set; }
        public RoomType SelectedRoomType { get; set; }
        public int RoomCount { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public string SpecialRequest { get; set; }

        // Danh sách
        public ObservableCollection<RoomType> RoomTypes { get; set; }
        public ObservableCollection<RoomDisplay> AvailableRooms { get; set; }
        public ObservableCollection<Room> SelectedRooms { get; set; }
        public ObservableCollection<Service> Services { get; set; }
        public ObservableCollection<Service> SelectedServices { get; set; }
        public ObservableCollection<Guest> Guests { get; set; }

        // Thông tin khách hàng
        public Guest SelectedGuest { get; set; }
        public string GuestSearchText { get; set; }

        // Tóm tắt đơn đặt phòng
        public string BookingSummary { get; set; }

        // Command
        public ICommand SearchRoomCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ConfirmBookingCommand { get; }
        public ICommand CancelBookingCommand { get; }
        public ICommand PrintBookingCommand { get; }

        // DbContext
        private readonly HotelManagementDbContext _dbContext;

        public BookingManagementViewModel()
        {
            _dbContext = new HotelManagementDbContext();
            RoomTypes = new ObservableCollection<RoomType>();
            AvailableRooms = new ObservableCollection<RoomDisplay>();
            SelectedRooms = new ObservableCollection<Room>();
            Services = new ObservableCollection<Service>();
            SelectedServices = new ObservableCollection<Service>();
            Guests = new ObservableCollection<Guest>();

            // Khởi tạo command (RelayCommand hoặc DelegateCommand tuỳ dự án)
            SearchRoomCommand = new RelayCommand(SearchRoom);
            ResetCommand = new RelayCommand(Reset);
            ConfirmBookingCommand = new RelayCommand(ConfirmBooking);
            CancelBookingCommand = new RelayCommand(CancelBooking);
            PrintBookingCommand = new RelayCommand(PrintBooking);
        }

        // Các phương thức xử lý logic (chưa triển khai)
        private void SearchRoom(object obj)
        {
            // TODO: Tìm phòng trống
        }

        private void Reset(object obj)
        {
            // TODO: Đặt lại điều kiện tìm kiếm
        }

        private void ConfirmBooking(object obj)
        {
            // TODO: Xác nhận đặt phòng
        }

        private void CancelBooking(object obj)
        {
            // TODO: Hủy thao tác đặt phòng
        }

        private void PrintBooking(object obj)
        {
            // TODO: In xác nhận đặt phòng
        }
    }
} 