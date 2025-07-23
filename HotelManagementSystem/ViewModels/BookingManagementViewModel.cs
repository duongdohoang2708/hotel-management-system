using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Windows.Threading;
using System.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class BookingManagementViewModel : ViewModelBase
    {
        // Điều kiện tìm kiếm
        private DateTime? _checkInDate;
        public DateTime? CheckInDate
        {
            get => _checkInDate;
            set
            {
                DateTime today = DateTime.Today;
                DateTime? newValue = value;
                if (newValue != null && newValue < today)
                {
                    CheckInDateError = "Can not be in the past!";
                    newValue = today;
                }
                else
                {
                    CheckInDateError = string.Empty;
                }
                if (SetProperty(ref _checkInDate, newValue))
                {
                    // Nếu CheckOutDate <= CheckInDate thì reset CheckOutDate
                    if (_checkOutDate != null && _checkOutDate <= _checkInDate)
                    {
                        CheckOutDate = null;
                        CheckOutDateError = "Must be after check-in!";
                    }
                    else
                    {
                        CheckOutDateError = string.Empty;
                    }
                    OnPropertyChanged(nameof(SelectedCheckInText));
                    OnPropertyChanged(nameof(SelectedCheckOutText));
                    SearchRoom();
                }
            }
        }

        private DateTime? _checkOutDate;
        public DateTime? CheckOutDate
        {
            get => _checkOutDate;
            set
            {
                DateTime? newValue = value;
                if (newValue != null && (_checkInDate == null || newValue <= _checkInDate))
                {
                    CheckOutDateError = "Must be after check-in!";
                    newValue = null;
                }
                else
                {
                    CheckOutDateError = string.Empty;
                }
                if (SetProperty(ref _checkOutDate, newValue))
                {
                    OnPropertyChanged(nameof(SelectedCheckInText));
                    OnPropertyChanged(nameof(SelectedCheckOutText));
                    SearchRoom();
                }
            }
        }

        private string _checkInDateError;
        public string CheckInDateError
        {
            get => _checkInDateError;
            set => SetProperty(ref _checkInDateError, value);
        }
        private string _checkOutDateError;
        public string CheckOutDateError
        {
            get => _checkOutDateError;
            set => SetProperty(ref _checkOutDateError, value);
        }

        public string SelectedCheckInText
        {
            get
            {
                if (CheckInDate == null || CheckInDate < DateTime.Today)
                    return string.Empty;
                return CheckInDate.Value.ToString("dd/MM/yyyy");
            }
        }
        public string SelectedCheckOutText
        {
            get
            {
                if (CheckOutDate == null || CheckInDate == null || CheckOutDate <= CheckInDate)
                    return string.Empty;
                return CheckOutDate.Value.ToString("dd/MM/yyyy");
            }
        }
        public class RoomTypeWrapper
        {
            public RoomType RoomType { get; set; }
            public string DisplayName { get; set; }
        }
        public ObservableCollection<RoomTypeWrapper> FilterRoomTypes { get; set; } = new ObservableCollection<RoomTypeWrapper>();
        private RoomTypeWrapper _selectedRoomType;
        public RoomTypeWrapper SelectedRoomType
        {
            get => _selectedRoomType;
            set
            {
                if (SetProperty(ref _selectedRoomType, value))
                {
                    SearchRoom();
                }
            }
        }
        public int RoomCount { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public string SpecialRequest { get; set; }

        // Danh sách
        public ObservableCollection<RoomType> RoomTypes { get; set; }
        public ObservableCollection<RoomDisplayForRoomManagement> AvailableRooms { get; set; }
        public ObservableCollection<RoomDisplayForRoomManagement> SelectedRooms { get; set; }
        public ObservableCollection<Service> Services { get; set; }
        public ObservableCollection<Service> SelectedServices { get; set; }
        public ObservableCollection<Guest> Guests { get; set; }

        // Thêm class BookingDisplay cho DataGrid
        public class BookingDisplay
        {
            public DateTime? BookingDate { get; set; }
            public string GuestName { get; set; }
            public string GuestIdCard { get; set; }
            public string GuestAddress { get; set; }
            public int NumberOfRoom { get; set; }
            public DateTime? CheckInDate { get; set; }
            public DateTime? CheckOutDate { get; set; }
            public decimal? Deposit { get; set; }
            public ObservableCollection<RoomDisplay> Rooms { get; set; } = new ObservableCollection<RoomDisplay>();
        }

        private BookingDisplay _selectedPendingBooking;
        public BookingDisplay SelectedPendingBooking
        {
            get => _selectedPendingBooking;
            set
            {
                if (SetProperty(ref _selectedPendingBooking, value))
                {
                    PendingBookingSelectedRooms.Clear();
                    if (value?.Rooms != null)
                    {
                        foreach (var room in value.Rooms)
                        {
                            PendingBookingSelectedRooms.Add(room);
                        }
                    }
                }
            }
        }

        // Danh sách các booking đang chờ
        public ObservableCollection<BookingDisplay> PendingBookings { get; set; } = new ObservableCollection<BookingDisplay>();
        public ObservableCollection<RoomDisplay> PendingBookingSelectedRooms { get; set; } = new ObservableCollection<RoomDisplay>();

        // Thông tin khách hàng
        private Guest _selectedGuest;
        public Guest SelectedGuest
        {
            get => _selectedGuest;
            set
            {
                if (SetProperty(ref _selectedGuest, value))
                {
                    OnPropertyChanged(nameof(SelectedGuestName));
                    OnPropertyChanged(nameof(SelectedGuestIdCard));
                    OnPropertyChanged(nameof(SelectedGuestAddress));
                }
            }
        }

        public string SelectedGuestName => SelectedGuest?.FullName ?? string.Empty;
        public string SelectedGuestIdCard => SelectedGuest?.IdCardNo ?? string.Empty;
        public string SelectedGuestAddress => SelectedGuest?.Address ?? string.Empty;
        
        private string _guestSearchKeyword = string.Empty;
        private DispatcherTimer _guestSearchTimer;
        public string GuestSearchKeyword
        {
            get => _guestSearchKeyword;
            set
            {
                if (SetProperty(ref _guestSearchKeyword, value))
                {
                    _guestSearchTimer?.Stop();
                    _guestSearchTimer?.Start();
                }
            }
        }

        public ICommand GuestSearchCommand { get; }
        public ICommand GuestClearSearchCommand { get; }

        private string _filterMaxPrice;
        public string FilterMaxPrice
        {
            get => _filterMaxPrice;
            set => SetProperty(ref _filterMaxPrice, value);
        }
        public ICommand FilterMaxPriceEnterCommand { get; }

        // Tóm tắt đơn đặt phòng
        public string BookingSummary { get; set; }

        // Command
        public ICommand SearchRoomCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ConfirmBookingCommand { get; }
        public ICommand CancelBookingCommand { get; }
        public ICommand PrintBookingCommand { get; }
        public ICommand AddRoomCommand { get; }
        public ICommand CancelPendingBookingCommand { get; }

        // DbContext
        private readonly HotelManagementDbContext _dbContext;

        public decimal TotalRoomPricePerDay
        {
            get
            {
                if (SelectedRooms == null) return 0;
                return SelectedRooms.Sum(r => r.Price);
            }
        }

        private string _bookingSearchKeyword = string.Empty;
        public string BookingSearchKeyword
        {
            get => _bookingSearchKeyword;
            set
            {
                if (SetProperty(ref _bookingSearchKeyword, value))
                {
                    LoadPendingBookings();
                }
            }
        }

        private decimal _deposit = 0;
        public decimal Deposit
        {
            get => _deposit;
            set => SetProperty(ref _deposit, value);
        }

        public BookingManagementViewModel()
        {
            _dbContext = new HotelManagementDbContext();
            RoomTypes = new ObservableCollection<RoomType>();
            FilterRoomTypes = new ObservableCollection<RoomTypeWrapper>();
            AvailableRooms = new ObservableCollection<RoomDisplayForRoomManagement>();
            SelectedRooms = new ObservableCollection<RoomDisplayForRoomManagement>();
            Services = new ObservableCollection<Service>();
            SelectedServices = new ObservableCollection<Service>();
            Guests = new ObservableCollection<Guest>();
            PendingBookings = new ObservableCollection<BookingDisplay>();
            Deposit = 0;
            // Khởi tạo timer cho tìm kiếm tự động
            _guestSearchTimer = new DispatcherTimer();
            _guestSearchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _guestSearchTimer.Tick += (s, e) =>
            {
                _guestSearchTimer.Stop();
                LoadGuests();
            };
            // Khởi tạo command (RelayCommand hoặc DelegateCommand tuỳ dự án)
            SearchRoomCommand = new RelayCommand(_ => SearchRoom());
            ResetCommand = new RelayCommand(Reset);
            ConfirmBookingCommand = new RelayCommand(ConfirmBooking);
            CancelBookingCommand = new RelayCommand(CancelBooking);
            PrintBookingCommand = new RelayCommand(PrintBooking);
            GuestSearchCommand = new RelayCommand(_ => LoadGuests());
            GuestClearSearchCommand = new RelayCommand(_ =>
            {
                GuestSearchKeyword = string.Empty;
                LoadGuests();
            });
            FilterMaxPriceEnterCommand = new RelayCommand(_ => SearchRoom());
            LoadRoomTypes();
            // Thêm mục All vào đầu FilterRoomTypes
            FilterRoomTypes.Add(new RoomTypeWrapper { RoomType = null, DisplayName = "Tất cả" });
            foreach (var type in RoomTypes)
            {
                FilterRoomTypes.Add(new RoomTypeWrapper { RoomType = type, DisplayName = type.TypeName });
            }
            SelectedRoomType = FilterRoomTypes.FirstOrDefault();
            LoadGuests();
            // Nếu chưa có ngày nhận và trả, mặc định là hôm nay và hôm nay + 1
            if (CheckInDate == null)
                CheckInDate = DateTime.Today;
            if (CheckOutDate == null)
                CheckOutDate = DateTime.Today.AddDays(1);
            SearchRoom();
            AddRoomCommand = new RelayCommand(roomObj => AddRoomToSelected(roomObj as RoomDisplayForRoomManagement));
            SelectedRooms.CollectionChanged += (s, e) =>
            {
                if (e.NewItems != null)
                {
                    foreach (var item in e.NewItems)
                    {
                        if (item is RoomDisplayForRoomManagement room)
                        {
                            room.PropertyChanged += RoomDisplay_PropertyChanged;
                        }
                    }
                }
                if (e.OldItems != null)
                {
                    foreach (var item in e.OldItems)
                    {
                        if (item is RoomDisplayForRoomManagement room)
                        {
                            room.PropertyChanged -= RoomDisplay_PropertyChanged;
                        }
                    }
                }
                OnPropertyChanged(nameof(TotalRoomPricePerDay));
            };
            LoadPendingBookings();
            CancelPendingBookingCommand = new RelayCommand(_ => CancelPendingBooking(), _ => SelectedPendingBooking != null);
        }

        private void RoomDisplay_PropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            if (e.PropertyName == nameof(RoomDisplayForRoomManagement.Price))
            {
                OnPropertyChanged(nameof(TotalRoomPricePerDay));
            }
        }

        private void LoadRoomTypes()
        {
            RoomTypes.Clear();
            var roomTypes = _dbContext.RoomTypes.ToList();
            foreach (var type in roomTypes)
            {
                RoomTypes.Add(type);
            }
        }
        
        private void LoadGuests()
        {
            var query = _dbContext.Guests.AsQueryable();
            if (!string.IsNullOrWhiteSpace(GuestSearchKeyword))
            {
                string keyword = GuestSearchKeyword.Trim().ToLower();
                query = query.Where(g =>
                    (g.FullName != null && g.FullName.ToLower().Contains(keyword)) ||
                    (g.Phone != null && g.Phone.ToLower().Contains(keyword)) ||
                    (g.IdCardNo != null && g.IdCardNo.ToLower().Contains(keyword))
                );
            }
            var guests = query.ToList();
            Guests.Clear();
            foreach (var guest in guests)
            {
                Guests.Add(guest);
            }
        }
        
        // Các phương thức xử lý logic (chưa triển khai)
        private void SearchRoom()
        {
            if (CheckInDate == null || CheckOutDate == null || CheckOutDate <= CheckInDate)
            {
                AvailableRooms.Clear();
                return;
            }
            var query = _dbContext.Rooms
                .Where(r => r.Status == "Trống");
            // Nếu không phải All thì lọc theo loại phòng
            if (SelectedRoomType != null && SelectedRoomType.RoomType != null)
            {
                query = query.Where(r => r.RoomTypeId == SelectedRoomType.RoomType.RoomTypeId);
            }
            // Lọc theo giá phòng nếu có nhập
            if (!string.IsNullOrWhiteSpace(FilterMaxPrice) && decimal.TryParse(FilterMaxPrice, out decimal maxPrice))
            {
                query = query.Where(r => r.RoomType.BasePrice != null && r.RoomType.BasePrice <= maxPrice);
            }
            var checkIn = CheckInDate.Value.Date;
            var checkOut = CheckOutDate.Value.Date;
            var availableRooms = query.Where(r =>
                !r.BookedRooms.Any(br =>
                    br.Booking != null &&
                    br.Booking.CheckIn != null &&
                    br.Booking.CheckOut != null &&
                    br.Booking.CheckIn < checkOut && br.Booking.CheckOut > checkIn
                )
            ).ToList();
            AvailableRooms.Clear();
            foreach (var room in availableRooms)
            {
                if (SelectedRooms.Any(r => r.RoomId == room.RoomId))
                    continue; // Bỏ qua phòng đã chọn
                var roomDisplay = new RoomDisplayForRoomManagement
                {
                    RoomId = room.RoomId,
                    RoomNumber = room.RoomNumber,
                    RoomType = room.RoomType.TypeName,
                    Price = room.RoomType.BasePrice ?? 0,
                    IsSelected = false
                };
                AvailableRooms.Add(roomDisplay);
            }
        }

        private void AddRoomToSelected(RoomDisplayForRoomManagement room)
        {
            if (room == null) return;
            if (!SelectedRooms.Any(r => r.RoomId == room.RoomId))
            {
                var selectedRoom = new RoomDisplayForRoomManagement
                {
                    RoomId = room.RoomId,
                    RoomNumber = room.RoomNumber,
                    RoomType = room.RoomType,
                    Price = room.Price,
                    IsSelected = false
                };
                SelectedRooms.Add(selectedRoom);
                var toRemove = AvailableRooms.FirstOrDefault(r => r.RoomId == room.RoomId);
                if (toRemove != null)
                    AvailableRooms.Remove(toRemove);
            }
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
            SelectedRooms.Clear();      // Xóa toàn bộ phòng đã chọn
            SelectedGuest = null;       // Xóa thông tin khách hàng đang chọn
            SearchRoom();
        }

        private void PrintBooking(object obj)
        {
            // TODO: In xác nhận đặt phòng
        }

        // Load các booking đang chờ (ví dụ: StatusId = 1)
        private void LoadPendingBookings()
        {
            PendingBookings.Clear();
            var pendingQuery = _dbContext.Bookings
                .Include(b => b.Guest)
                .Include(b => b.BookedRooms).ThenInclude(br => br.Room).ThenInclude(r => r.RoomType)
                .Where(b => b.StatusId == 1); // Giả sử 1 là trạng thái "Chờ xác nhận"
            if (!string.IsNullOrWhiteSpace(BookingSearchKeyword))
            {
                string keyword = BookingSearchKeyword.Trim().ToLower();
                pendingQuery = pendingQuery.Where(b => b.Guest.FullName.ToLower().Contains(keyword));
            }
            var pending = pendingQuery
                .OrderBy(b => b.CheckIn)
                .ToList();
            foreach (var b in pending)
            {
                var bookingDisplay = new BookingDisplay
                {
                    BookingDate = b.BookingDate,
                    GuestName = b.Guest?.FullName ?? string.Empty,
                    GuestIdCard = b.Guest?.IdCardNo ?? string.Empty,
                    GuestAddress = b.Guest?.Address ?? string.Empty,
                    NumberOfRoom = b.BookedRooms?.Count ?? 0,
                    CheckInDate = b.CheckIn,
                    CheckOutDate = b.CheckOut,
                    Deposit = b.Deposit,
                    Rooms = new ObservableCollection<RoomDisplay>(
                        b.BookedRooms.Select(br => new RoomDisplay
                        {
                            RoomId = br.Room.RoomId,
                            RoomNumber = br.Room.RoomNumber,
                            RoomTypeName = br.Room.RoomType?.TypeName ?? string.Empty,
                            Price = br.RoomPrice ?? br.Room.RoomType?.BasePrice ?? 0
                        })
                    )
                };
                PendingBookings.Add(bookingDisplay);
            }
        }

        private void CancelPendingBooking()
        {
            if (SelectedPendingBooking == null) return;
            string guestName = SelectedPendingBooking.GuestName;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn hủy đặt phòng của khách hàng '{guestName}' không?", "Xác nhận hủy đặt phòng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                using (var db = new HotelManagementDbContext())
                {
                    // Tìm booking theo ngày đặt, tên khách, ngày nhận, ngày trả, tiền cọc (hoặc nếu có BookingId thì dùng)
                    var booking = db.Bookings
                        .Include(b => b.BookedRooms)
                        .FirstOrDefault(b =>
                            b.BookingDate == SelectedPendingBooking.BookingDate &&
                            b.Guest.FullName == SelectedPendingBooking.GuestName &&
                            b.CheckIn == SelectedPendingBooking.CheckInDate &&
                            b.CheckOut == SelectedPendingBooking.CheckOutDate &&
                            b.Deposit == SelectedPendingBooking.Deposit
                        );
                    if (booking != null)
                    {
                        // Xóa các BookedRoom liên quan
                        db.BookedRooms.RemoveRange(booking.BookedRooms);
                        db.Bookings.Remove(booking);
                        db.SaveChanges();
                    }
                }
                // Xóa khỏi danh sách hiển thị
                PendingBookings.Remove(SelectedPendingBooking);
                SelectedPendingBooking = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi khi hủy đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 