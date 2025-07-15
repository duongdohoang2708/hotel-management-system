using System;
using System.Collections.ObjectModel;
using HotelManagementSystem.Models;
using System.Windows.Input;
using System.Linq;
using HotelManagementSystem.Views.Windows;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.ViewModels
{
    public class RoomDetailViewModel : ViewModelBase
    {
        public string RoomNumber { get; }
        public string? GuestName { get; }
        public DateTime? CheckInPlan { get; }
        public string? StayDuration { get; }
        public int? GuestCount { get; }
        public string Status { get; }
        public string CleanStatus { get; set; }
        public ObservableCollection<string> CleanStatusOptions { get; } = new ObservableCollection<string> { "Đã dọn dẹp", "Chưa dọn dẹp", "Sửa chữa" };
        public ObservableCollection<ServiceItem> Services { get; } = new ObservableCollection<ServiceItem>();

        // Command thao tác
        public ICommand CheckInCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand AddServiceCommand { get; }
        public ICommand CheckoutCommand { get; }
        public ICommand CloseCommand { get; }

        // Property hiển thị nút
        public bool ShowCheckInButton => Status == "Phòng đã đặt";
        public bool ShowAddServiceButton => Status == "Phòng đang thuê";
        public bool ShowCheckoutButton => Status == "Phòng đang thuê";

        public event Action? RequestClose;
        public event Action? RequestReloadRoomStatus;

        public RoomDetailViewModel(RoomStatusItem room)
        {
            RoomNumber = room.RoomNumber;
            GuestName = room.GuestName;
            CheckInPlan = room.CheckInPlan;
            StayDuration = room.StayDuration;
            Status = room.Status;
            CleanStatus = room.CleanStatus;
            GuestCount = room.GuestCount;

            // Lấy danh sách dịch vụ thực tế từ DB
            using (var db = new HotelManagementDbContext())
            {
                var bookedRoom = db.BookedRooms
                    .Include(br => br.Booking)
                        .ThenInclude(b => b.Guest)
                    .Where(br => br.Room.RoomNumber == RoomNumber && br.Booking != null)
                    .OrderByDescending(br => br.Booking.CheckIn)
                    .FirstOrDefault();
                if (bookedRoom != null && bookedRoom.Booking != null)
                {
                    var services = db.RoomServiceUsages
                        .Where(s => s.BookedRoomId == bookedRoom.BookedRoomId)
                        .Select(s => new ServiceItem
                        {
                            ServiceName = s.Service.ServiceName,
                            Quantity = s.Quantity,
                            TotalPrice = s.UnitPrice * s.Quantity
                        }).ToList();
                    foreach (var s in services)
                        Services.Add(s);
                }
            }

            CheckInCommand = new RelayCommand(_ => OnCheckIn(), _ => ShowCheckInButton);
            SaveCommand = new RelayCommand(_ => OnSave());
            AddServiceCommand = new RelayCommand(_ => OnAddService(), _ => ShowAddServiceButton);
            CheckoutCommand = new RelayCommand(_ => OnCheckout(), _ => ShowCheckoutButton);
            CloseCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void OnCheckIn()
        {
            // Cập nhật trạng thái phòng sang "Phòng đang thuê" trong DB
            using (var db = new HotelManagementDbContext())
            {
                var room = db.Rooms.FirstOrDefault(r => r.RoomNumber == RoomNumber);
                if (room != null)
                {
                    room.Status = "Phòng đang thuê";
                    db.SaveChanges();
                }
            }
            System.Windows.MessageBox.Show("Nhận phòng thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            // Gửi event reload lại danh sách phòng
            RequestReloadRoomStatus?.Invoke();
            // Đóng cửa sổ chi tiết phòng
            RequestClose?.Invoke();
        }
        private void OnSave()
        {
            // Lưu trạng thái dọn dẹp vào DB
            using (var db = new HotelManagementDbContext())
            {
                var room = db.Rooms.FirstOrDefault(r => r.RoomNumber == RoomNumber);
                if (room != null)
                {
                    room.CleanStatus = CleanStatus;
                    db.SaveChanges();
                }
            }
            RequestReloadRoomStatus?.Invoke();
        }
        private void OnAddService()
        {
            // Lấy RoomId và BookingId hiện tại
            int roomId = 0;
            int bookingId = 0;
            using (var db = new HotelManagementDbContext())
            {
                var room = db.Rooms.FirstOrDefault(r => r.RoomNumber == RoomNumber);
                if (room != null)
                {
                    roomId = room.RoomId;
                    // Lấy BookedRoom gần nhất
                    var bookedRoom = db.BookedRooms
                        .Include(br => br.Booking)
                        .Where(br => br.RoomId == room.RoomId && br.Booking != null)
                        .OrderByDescending(br => br.Booking.CheckIn)
                        .FirstOrDefault();
                    if (bookedRoom != null && bookedRoom.Booking != null)
                        bookingId = bookedRoom.Booking.BookingId;
                }
            }
            if (roomId == 0 || bookingId == 0) return;
            // Mở cửa sổ thêm dịch vụ
            var addServiceWindow = new AddServiceWindow();
            var addServiceVm = new AddServiceViewModel(roomId, bookingId);
            addServiceWindow.DataContext = addServiceVm;
            addServiceVm.RequestReloadRoomDetail += () => ReloadServices();
            addServiceWindow.ShowDialog();
        }

        private void ReloadServices()
        {
            Services.Clear();
            using (var db = new HotelManagementDbContext())
            {
                var room = db.Rooms.FirstOrDefault(r => r.RoomNumber == RoomNumber);
                if (room != null)
                {
                    var bookedRoom = db.BookedRooms
                        .Include(br => br.Booking)
                        .Where(br => br.RoomId == room.RoomId && br.Booking != null)
                        .OrderByDescending(br => br.Booking.CheckIn)
                        .FirstOrDefault();
                    if (bookedRoom != null && bookedRoom.Booking != null)
                    {
                        var services = db.RoomServiceUsages
                            .Where(s => s.BookedRoomId == bookedRoom.BookedRoomId)
                            .Select(s => new ServiceItem
                            {
                                ServiceName = s.Service.ServiceName,
                                Quantity = s.Quantity,
                                TotalPrice = s.UnitPrice * s.Quantity
                            }).ToList();
                        foreach (var s in services)
                            Services.Add(s);
                    }
                }
            }
            // Sau khi thêm dịch vụ, reload lại RoomStatus nếu cần
            RequestReloadRoomStatus?.Invoke();
        }
        private void OnCheckout()
        {
            // TODO: Xử lý thanh toán
            RequestReloadRoomStatus?.Invoke();
            RequestClose?.Invoke();
        }
    }

    public class ServiceItem
    {
        public string ServiceName { get; set; } = string.Empty;
        public int Quantity { get; set; }
        public decimal TotalPrice { get; set; }
    }
}
