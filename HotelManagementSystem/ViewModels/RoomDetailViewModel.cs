using System;
using System.Collections.ObjectModel;
using HotelManagementSystem.Models;
using System.Windows.Input;
using System.Linq;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class RoomDetailViewModel : ViewModelBase
    {
        public string RoomNumber { get; }
        public string? GuestName { get; }
        public DateTime? CheckInPlan { get; }
        public string? StayDuration { get; }
        public int GuestCount { get; } = 1;
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

            // Lấy danh sách dịch vụ thực tế từ DB
            using (var db = new HotelManagementDbContext())
            {
                var reservationRoom = db.ReservationRooms
                    .Where(rr => rr.Room.RoomNumber == RoomNumber)
                    .OrderByDescending(rr => rr.Reservation.CheckInPlan)
                    .FirstOrDefault();
                if (reservationRoom != null)
                {
                    var services = db.ReservationRoomServices
                        .Where(s => s.ReservationId == reservationRoom.ReservationId && s.RoomId == reservationRoom.RoomId)
                        .Select(s => new ServiceItem
                        {
                            ServiceName = s.Service.ServiceName,
                            Quantity = s.Qty,
                            TotalPrice = s.UnitPrice * s.Qty
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
            // Lấy RoomId và ReservationId hiện tại
            int roomId = 0;
            int reservationId = 0;
            using (var db = new HotelManagementDbContext())
            {
                var room = db.Rooms.FirstOrDefault(r => r.RoomNumber == RoomNumber);
                if (room != null)
                {
                    roomId = room.RoomId;
                    // Lấy reservation gần nhất
                    var reservationRoom = db.ReservationRooms
                        .Where(rr => rr.RoomId == room.RoomId)
                        .OrderByDescending(rr => rr.Reservation.CheckInPlan)
                        .FirstOrDefault();
                    if (reservationRoom != null)
                        reservationId = reservationRoom.ReservationId;
                }
            }
            if (roomId == 0 || reservationId == 0) return;
            // Mở cửa sổ thêm dịch vụ
            var addServiceWindow = new AddServiceWindow();
            var addServiceVm = new AddServiceViewModel(roomId, reservationId);
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
                    var reservationRoom = db.ReservationRooms
                        .Where(rr => rr.RoomId == room.RoomId)
                        .OrderByDescending(rr => rr.Reservation.CheckInPlan)
                        .FirstOrDefault();
                    if (reservationRoom != null)
                    {
                        var services = db.ReservationRoomServices
                            .Where(s => s.ReservationId == reservationRoom.ReservationId && s.RoomId == reservationRoom.RoomId)
                            .Select(s => new ServiceItem
                            {
                                ServiceName = s.Service.ServiceName,
                                Quantity = s.Qty,
                                TotalPrice = s.UnitPrice * s.Qty
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
