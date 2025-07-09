using System.Collections.ObjectModel;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using System.Windows.Input;

namespace HotelManagementSystem.ViewModels
{
    public class RoomStatusViewModel : ViewModelBase
    {
        private ObservableCollection<RoomStatusItem> _rooms;
        public ObservableCollection<RoomStatusItem> Rooms
        {
            get => _rooms;
            set => SetProperty(ref _rooms, value);
        }

        private string _selectedStatus = "Tất cả";
        public string SelectedStatus
        {
            get => _selectedStatus;
            set { if (SetProperty(ref _selectedStatus, value)) OnFilterChanged(); }
        }

        private string _selectedCleanStatus = "Tất cả";
        public string SelectedCleanStatus
        {
            get => _selectedCleanStatus;
            set { if (SetProperty(ref _selectedCleanStatus, value)) OnFilterChanged(); }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set { if (SetProperty(ref _searchText, value)) OnFilterChanged(); }
        }

        private void OnFilterChanged()
        {
            OnPropertyChanged(nameof(SingleRooms));
            OnPropertyChanged(nameof(DoubleRooms));
            OnPropertyChanged(nameof(FamilyRooms));
        }

        private bool IsMatch(RoomStatusItem room)
        {
            // So sánh trực tiếp với giá trị tiếng Việt
            bool statusMatch = SelectedStatus == "Tất cả phòng" || SelectedStatus == "Tất cả" || room.Status == SelectedStatus;
            bool cleanMatch = SelectedCleanStatus == "Tất cả" || room.CleanStatus == SelectedCleanStatus;
            bool searchMatch = string.IsNullOrWhiteSpace(SearchText) || (room.RoomNumber?.Contains(SearchText, System.StringComparison.OrdinalIgnoreCase) == true);
            return statusMatch && cleanMatch && searchMatch;
        }

        public IEnumerable<RoomStatusItem> SingleRooms => Rooms.Where(r => r.RoomType == "Đơn" && IsMatch(r));
        public IEnumerable<RoomStatusItem> DoubleRooms => Rooms.Where(r => r.RoomType == "Đôi" && IsMatch(r));
        public IEnumerable<RoomStatusItem> FamilyRooms => Rooms.Where(r => r.RoomType == "Gia đình" && IsMatch(r));

        public Action<RoomStatusItem>? OpenRoomDetailAction { get; set; }
        public ICommand OpenRoomDetailCommand { get; }

        public RoomStatusViewModel()
        {
            using var db = new HotelManagementSystem.Models.HotelManagementDbContext();
            var now = DateTime.Now;
            var roomList = db.Rooms
                .Include(r => r.RoomType)
                .Include(r => r.ReservationRooms)
                    .ThenInclude(rr => rr.Reservation)
                        .ThenInclude(res => res.Customer)
                .ToList();

            var items = new List<RoomStatusItem>();
            foreach (var room in roomList)
            {
                // Lấy reservation gần nhất (reservation có CheckInPlan lớn nhất)
                var latestReservationRoom = room.ReservationRooms
                    .OrderByDescending(rr => rr.Reservation.CheckInPlan)
                    .FirstOrDefault();
                string? guestName = latestReservationRoom?.Reservation.Customer.FullName;
                DateTime? checkIn = latestReservationRoom?.Reservation.CheckInPlan;
                DateTime? checkOut = latestReservationRoom?.Reservation.CheckOutPlan;
                string? stayDuration = null;
                if (checkIn.HasValue && checkOut.HasValue)
                {
                    int days = (checkOut.Value - checkIn.Value).Days;
                    stayDuration = days > 0 ? $"{days} ngày" : "1 ngày";
                }

                // Màu nền và màu chữ card theo trạng thái phòng (màu nhạt)
                (string cardBg, string cardFg) = room.Status switch
                {
                    "Phòng trống" => ("#A5D6A7", "#222"), // Xanh lá nhạt
                    "Phòng đã đặt" => ("#FFD180", "#222"), // Cam nhạt
                    "Phòng đang thuê" => ("#90CAF9", "#222"), // Xanh dương nhạt
                    "Sửa chữa" => ("#FFAB91", "#222"), // Đỏ/cam nhạt
                    _ => ("#FFF", "#222")
                };
                // Icon và màu icon theo trạng thái dọn dẹp
                (string icon, string iconColor) = room.CleanStatus switch
                {
                    "Đã dọn dẹp" => ("Check", "#388E3C"),
                    "Chưa dọn dẹp" => ("Close", "#D32F2F"),
                    "Sửa chữa" => ("Wrench", "#FFA000"),
                    _ => ("Help", "#757575")
                };

                items.Add(new RoomStatusItem
                {
                    RoomId = room.RoomId,
                    RoomNumber = room.RoomNumber,
                    RoomType = room.RoomType.TypeName,
                    Status = room.Status,
                    CleanStatus = room.CleanStatus,
                    GuestName = guestName,
                    CheckInPlan = checkIn,
                    CheckOutPlan = checkOut,
                    StayDuration = stayDuration,
                    CardBackground = cardBg,
                    CardForeground = cardFg,
                    CleanStatusIcon = icon,
                    CleanStatusIconColor = iconColor
                });
            }
            Rooms = new ObservableCollection<RoomStatusItem>(items);
            OpenRoomDetailCommand = new RelayCommand(item =>
            {
                if (item is RoomStatusItem room)
                    OpenRoomDetailAction?.Invoke(room);
            });
        }
    }
} 