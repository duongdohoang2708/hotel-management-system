using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Windows.Threading;
using System.Windows;
using HotelManagementSystem.Views.Windows;

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
            public int BookingId { get; set; }
            public int GuestId { get; set; }
            public DateTime? BookingDate { get; set; }
            public string GuestName { get; set; }
            public string GuestIdCard { get; set; }
            public string GuestAddress { get; set; }
            public int NumberOfRoom { get; set; }
            public DateTime? CheckInDate { get; set; }
            public DateTime? CheckOutDate { get; set; }
            public decimal? Deposit { get; set; }
            public ObservableCollection<RoomDisplay> Rooms { get; set; } = new ObservableCollection<RoomDisplay>();
            public int? StatusId { get; set; }
            public string StatusName { get; set; }
        }

        private BookingDisplay _SelectedBooking;
        public BookingDisplay SelectedBooking
        {
            get => _SelectedBooking;
            set
            {
                if (SetProperty(ref _SelectedBooking, value))
                {
                    BookingselectedRooms.Clear();
                    if (value?.Rooms != null)
                    {
                        foreach (var room in value.Rooms)
                        {
                            BookingselectedRooms.Add(room);
                        }
                    }
                    // Cập nhật trạng thái enable/disable các nút lệnh
                    if (value == null)
                    {
                        CanEditBooking = false;
                        CanDeleteBooking = false;
                        CanConfirmBookingButton = false;
                        CanCancelBooking = false;
                        CanNoShowBooking = false;
                    }
                    else
                    {
                        CanEditBooking = true;
                        CanDeleteBooking = true;
                        int status = value.StatusId ?? 0;
                        // Quy tắc điều khiển nút lệnh dựa trên trạng thái
                        if (status == 1) //Đặt phòng
                        {
                            CanConfirmBookingButton = false;
                            CanCancelBooking = true;
                            CanNoShowBooking = true;
                        }
                        else if (status == 2 || status == 3) //Đã nhận phòng hoặc đã trả phòng
                        {
                            CanConfirmBookingButton = false;
                            CanCancelBooking = false;
                            CanNoShowBooking = false;
                        }
                        else if (status == 4) //Không đến nhận phòng
                        {
                            CanConfirmBookingButton = true;
                            CanCancelBooking = false;
                            CanNoShowBooking = true;
                        }
                        else if (status == 5) // Đã hủy phòng
                        {
                            CanConfirmBookingButton = true;
                            CanCancelBooking = true;
                            CanNoShowBooking = false;
                        }
                        else if (status > 5) //Các trạng thái khác (ví dụ: đã thanh toán, đã hoàn thành)
                        {
                            CanConfirmBookingButton = false;
                            CanCancelBooking = false;
                            CanNoShowBooking = false;
                        }
                        else
                        {
                            CanConfirmBookingButton = false;
                            CanCancelBooking = false;
                            CanNoShowBooking = false;
                        }
                    }
                }
            }
        }

        // Danh sách các booking đang chờ
        public ObservableCollection<BookingDisplay> Bookings { get; set; } = new ObservableCollection<BookingDisplay>();
        public ObservableCollection<RoomDisplay> BookingselectedRooms { get; set; } = new ObservableCollection<RoomDisplay>();

        // Thuộc tính cho dòng được chọn trong BookingRoomsDataGrid
        private RoomDisplay _selectedBookingRoom;
        public RoomDisplay SelectedBookingRoom
        {
            get => _selectedBookingRoom;
            set
            {
                if (SetProperty(ref _selectedBookingRoom, value))
                {
                    OnPropertyChanged(nameof(CanDeleteBookingRoom));
                }
            }
        }
        // Thuộc tính bool để enable/disable nút Xóa phòng
        public bool CanDeleteBookingRoom => SelectedBookingRoom != null;

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
        private bool _isAddingGuest = false;
        public string GuestSearchKeyword
        {
            get => _guestSearchKeyword;
            set
            {
                if (SetProperty(ref _guestSearchKeyword, value))
                {
                    if (!_isAddingGuest)
                    {
                        _guestSearchTimer?.Stop();
                        _guestSearchTimer?.Start();
                    }
                }
            }
        }

        public ICommand GuestSearchCommand { get; }
        public ICommand GuestClearSearchCommand { get; }
        public ICommand AddGuestCommand { get; }

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
        
        public ICommand AddRoomCommand { get; }
        public ICommand DeleteBookingCommand { get; }
        public ICommand ConfirmSelectedBookingCommand { get; }
        public ICommand CancelSelectedBookingCommand { get; }
        public ICommand NoShowSelectedBookingCommand { get; }
        public ICommand EditBookingCommand { get; }
        public ICommand DeleteRoomCommand { get; }

        // Các thuộc tính điều khiển trạng thái enable/disable của các nút lệnh
        private bool _canEditBooking;
        public bool CanEditBooking
        {
            get => _canEditBooking;
            set => SetProperty(ref _canEditBooking, value);
        }
        private bool _canDeleteBooking;
        public bool CanDeleteBooking
        {
            get => _canDeleteBooking;
            set => SetProperty(ref _canDeleteBooking, value);
        }
        private bool _canConfirmBooking;
        public bool CanConfirmBookingButton
        {
            get => _canConfirmBooking;
            set => SetProperty(ref _canConfirmBooking, value);
        }
        private bool _canCancelBooking;
        public bool CanCancelBooking
        {
            get => _canCancelBooking;
            set => SetProperty(ref _canCancelBooking, value);
        }
        private bool _canNoShowBooking;
        public bool CanNoShowBooking
        {
            get => _canNoShowBooking;
            set => SetProperty(ref _canNoShowBooking, value);
        }

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
                    LoadBookings();
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
            Bookings = new ObservableCollection<BookingDisplay>();
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
            ConfirmBookingCommand = new RelayCommand(ConfirmBooking, _ => CanConfirmBooking());
            CancelBookingCommand = new RelayCommand(CancelBooking);
            
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
            AddRoomCommand = new RelayCommand(_ => AddRoomToBooking(), _ => SelectedBooking != null);
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
            LoadBookings();
            DeleteBookingCommand = new RelayCommand(_ => DeleteBooking(), _ => SelectedBooking != null);
            AddGuestCommand = new RelayCommand(_ =>
            {
                var vm = new HotelManagementSystem.ViewModels.GuestAddEditViewModel(false);
                var win = new HotelManagementSystem.Views.Windows.GuestAddEditWindow(vm);
                vm.GuestSaved += guest =>
                {
                    _isAddingGuest = true;
                    GuestSearchKeyword = guest.FullName ?? string.Empty;
                    LoadGuests();
                    Application.Current.Dispatcher.InvokeAsync(() =>
                    {
                        SelectedGuest = Guests.FirstOrDefault(g => g.GuestId == guest.GuestId);
                        _isAddingGuest = false;
                    }, System.Windows.Threading.DispatcherPriority.Background);
                };
                win.Owner = Application.Current.MainWindow;
                win.ShowDialog();
            });
            ConfirmSelectedBookingCommand = new RelayCommand(ConfirmSelectedBooking, _ => CanConfirmSelectedBooking());
            CancelSelectedBookingCommand = new RelayCommand(CancelSelectedBooking, _ => CanCancelSelectedBooking());
            NoShowSelectedBookingCommand = new RelayCommand(NoShowSelectedBooking, _ => CanNoShowSelectedBooking());
            EditBookingCommand = new RelayCommand(_ => EditBooking(), _ => SelectedBooking != null);
            DeleteRoomCommand = new RelayCommand(_ => DeleteRoom(), _ => CanDeleteBookingRoom);
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

        private bool CanConfirmBooking()
        {
            return SelectedGuest != null && SelectedRooms != null && SelectedRooms.Count > 0;
        }

        private void ConfirmBooking(object obj)
        {
            if (SelectedGuest == null || SelectedRooms == null || SelectedRooms.Count == 0)
                return;
            string guestName = SelectedGuest.FullName;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn đặt phòng cho khách hàng '{guestName}' không?", "Xác nhận đặt phòng", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                using (var db = new HotelManagementDbContext())
                {
                    // Tạo booking mới
                    var booking = new Booking
                    {
                        GuestId = SelectedGuest.GuestId,
                        BookingDate = DateTime.Now,
                        CheckIn = CheckInDate,
                        CheckOut = CheckOutDate,
                        Deposit = Deposit,
                        StatusId = 1, 
                        StaffId = AppSession.CurrentAccount.StaffId 
                    };
                    db.Bookings.Add(booking);
                    db.SaveChanges();
                    // Thêm các booked room
                    foreach (var room in SelectedRooms)
                    {
                        var bookedRoom = new BookedRoom
                        {
                            BookingId = booking.BookingId,
                            RoomId = room.RoomId,
                            RoomPrice = room.Price
                        };
                        db.BookedRooms.Add(bookedRoom);
                    }
                    db.SaveChanges();
                }
                // Sau khi thêm xong, cập nhật lại danh sách Bookings
                LoadBookings();
                // Lọc theo tên khách vừa đặt
                BookingSearchKeyword = SelectedGuest.FullName;
                LoadBookings();
                // Chọn dòng vừa thêm
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SelectedBooking = Bookings.FirstOrDefault(b => b.GuestId == SelectedGuest.GuestId && b.CheckInDate == CheckInDate && b.CheckOutDate == CheckOutDate);
                }, System.Windows.Threading.DispatcherPriority.Background);
                // Reset danh sách phòng đã chọn
                SelectedRooms.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBooking(object obj)
        {
            SelectedRooms.Clear();      // Xóa toàn bộ phòng đã chọn
            SelectedGuest = null;       // Xóa thông tin khách hàng đang chọn
            SearchRoom();
        }

      

        // Load  mọi booking
        private void LoadBookings()
        {
            Bookings.Clear();
            var bookingQuery = _dbContext.Bookings
                .Include(b => b.Guest)
                .Include(b=>b.Status)
                .Include(b => b.BookedRooms).ThenInclude(br => br.Room).ThenInclude(r => r.RoomType)
                .Where(b => b.StatusId <= 5); // Load all
            if (!string.IsNullOrWhiteSpace(BookingSearchKeyword))
            {
                string keyword = BookingSearchKeyword.Trim().ToLower();
                bookingQuery = bookingQuery.Where(b => b.Guest.FullName.ToLower().Contains(keyword));
            }
            var booking = bookingQuery
                .OrderBy(b => b.CheckIn)
                .ToList();
            foreach (var b in booking)
            {
                var bookingDisplay = new BookingDisplay
                {
                    BookingId = b.BookingId,
                    GuestId = b.GuestId,
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
                    ),
                    StatusId = b.StatusId,
                    StatusName = b.Status?.StatusName ?? string.Empty
                };
                Bookings.Add(bookingDisplay);
            }
        }

        private void DeleteBooking()
        {
            if (SelectedBooking == null) return;
            string guestName = SelectedBooking.GuestName;
            
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa dữ liệu đặt phòng của khách hàng '{guestName}' không?", "Xác nhận xóa đặt phòng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                using (var db = new HotelManagementDbContext())
                {
                    // Tìm booking theo ngày đặt, tên khách, ngày nhận, ngày trả, tiền cọc (hoặc nếu có BookingId thì dùng)
                    var booking = db.Bookings
                        .Include(b => b.BookedRooms)
                        .FirstOrDefault(b =>
                            b.BookingId == SelectedBooking.BookingId
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
                Bookings.Remove(SelectedBooking);
                SelectedBooking = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi khi hủy đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanConfirmSelectedBooking()
        {
            // Chỉ cho phép khi có booking được chọn và trạng thái khác 1 (chưa xác nhận)
            return SelectedBooking != null && SelectedBooking.StatusId != 1;
        }

        private void ConfirmSelectedBooking(object obj)
        {
            if (SelectedBooking == null) return;
            string guestName = SelectedBooking.GuestName;
            int selectedBookingId = SelectedBooking.BookingId;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xác nhận đặt phòng cho khách hàng '{guestName}' không?", "Xác nhận đặt phòng", MessageBoxButton.YesNo, MessageBoxImage.Question);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == SelectedBooking.BookingId);
                if (booking != null)
                {
                    booking.StatusId = 1; // Đặt lại trạng thái là 1 (đã xác nhận)
                    _dbContext.SaveChanges();
                }
                LoadBookings();
                // Giữ lại dòng vừa xác nhận
                SelectedBooking = Bookings.FirstOrDefault(b => b.BookingId == selectedBookingId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi xác nhận đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanCancelSelectedBooking()
        {
            return SelectedBooking != null && SelectedBooking.StatusId != 4;
        }
        private void CancelSelectedBooking(object obj)
        {
            if (SelectedBooking == null) return;
            string guestName = SelectedBooking.GuestName;
            int selectedBookingId = SelectedBooking.BookingId;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn hủy đặt phòng của khách hàng '{guestName}' không?", "Xác nhận hủy đặt phòng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == SelectedBooking.BookingId);
                if (booking != null)
                {
                    booking.StatusId = 4; // Hủy đặt phòng
                    _dbContext.SaveChanges();
                }
                LoadBookings();
                SelectedBooking = Bookings.FirstOrDefault(b => b.BookingId == selectedBookingId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi hủy đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private bool CanNoShowSelectedBooking()
        {
            return SelectedBooking != null && SelectedBooking.StatusId != 5;
        }
        private void NoShowSelectedBooking(object obj)
        {
            if (SelectedBooking == null) return;
            string guestName = SelectedBooking.GuestName;
            int selectedBookingId = SelectedBooking.BookingId;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn đánh dấu khách hàng '{guestName}' là không đến nhận phòng không?", "Xác nhận không đến nhận phòng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == SelectedBooking.BookingId);
                if (booking != null)
                {
                    booking.StatusId = 5; // Không đến nhận phòng
                    _dbContext.SaveChanges();
                }
                LoadBookings();
                SelectedBooking = Bookings.FirstOrDefault(b => b.BookingId == selectedBookingId);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi đánh dấu không đến nhận phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void EditBooking()
        {
            if (SelectedBooking == null) return;
            // Tạo bản sao Booking để sửa
            var booking = _dbContext.Bookings.Include(b => b.Guest).Include(b => b.Status).FirstOrDefault(b => b.BookingId == SelectedBooking.BookingId);
            if (booking == null) return;
            // Lấy danh sách Guest và Status
            var guests = new ObservableCollection<Guest>(_dbContext.Guests.ToList());
            var statuses = new ObservableCollection<BookingStatus>(_dbContext.BookingStatuses.ToList());
            var vm = new BookingAddEditViewModel(booking, guests, statuses);
            var win = new HotelManagementSystem.Views.Windows.BookingAddEditWindow(vm);
            if (win.ShowDialog() == true)
            {
                // Lưu thay đổi vào DB
                _dbContext.SaveChanges();
                LoadBookings();
                SelectedBooking = Bookings.FirstOrDefault(b => b.BookingId == booking.BookingId);
            }
        }

        private void DeleteRoom()
        {
            if (SelectedBooking == null || SelectedBookingRoom == null) return;
            string roomInfo = $"Phòng {SelectedBookingRoom.RoomNumber} - {SelectedBookingRoom.RoomTypeName}";
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa {roomInfo} khỏi đơn đặt phòng này không?", "Xác nhận xóa phòng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                using (var db = new HotelManagementDbContext())
                {
                    var bookedRoom = db.BookedRooms.FirstOrDefault(br => br.BookingId == SelectedBooking.BookingId && br.RoomId == SelectedBookingRoom.RoomId);
                    if (bookedRoom != null)
                    {
                        db.BookedRooms.Remove(bookedRoom);
                        db.SaveChanges();
                    }
                }
                // Xóa khỏi danh sách hiển thị
                BookingselectedRooms.Remove(SelectedBookingRoom);
                SelectedBooking.Rooms.Remove(SelectedBookingRoom);
                SelectedBookingRoom = null;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi khi xóa phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddRoomToBooking()
        {
            if (SelectedBooking == null) return;
            // Lấy khoảng thời gian của booking
            var checkIn = SelectedBooking.CheckInDate;
            var checkOut = SelectedBooking.CheckOutDate;
            if (checkIn == null || checkOut == null)
            {
                MessageBox.Show("Vui lòng chọn ngày nhận và ngày trả cho booking này!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            // Lấy danh sách phòng trống trong khoảng thời gian này
            ObservableCollection<RoomDisplay> availableRooms = new ObservableCollection<RoomDisplay>();
            using (var db = new HotelManagementDbContext())
            {
                var rooms = db.Rooms
                    .Include(r => r.RoomType)
                    .Where(r => r.Status == "Trống")
                    .ToList();
                foreach (var room in rooms)
                {
                    bool isBooked = db.BookedRooms.Any(br => br.RoomId == room.RoomId &&
                        br.Booking.CheckIn < checkOut && br.Booking.CheckOut > checkIn);
                    bool alreadyInBooking = SelectedBooking.Rooms.Any(r => r.RoomId == room.RoomId);
                    if (!isBooked && !alreadyInBooking)
                    {
                        availableRooms.Add(new RoomDisplay
                        {
                            RoomId = room.RoomId,
                            RoomNumber = room.RoomNumber,
                            RoomTypeName = room.RoomType?.TypeName ?? string.Empty,
                            Price = room.RoomType?.BasePrice ?? 0
                        });
                    }
                }
            }
            if (availableRooms.Count == 0)
            {
                MessageBox.Show("Không còn phòng trống phù hợp để thêm!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            // Hiển thị dialog chọn phòng
            var selectRoomWindow = new SelectRoomWindow(availableRooms);
            if (selectRoomWindow.ShowDialog() == true)
            {
                var selectedRoom = selectRoomWindow.SelectedRoom;
                if (selectedRoom == null) return;
                try
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var bookedRoom = new BookedRoom
                        {
                            BookingId = SelectedBooking.BookingId,
                            RoomId = selectedRoom.RoomId,
                            RoomPrice = selectedRoom.Price
                        };
                        db.BookedRooms.Add(bookedRoom);
                        db.SaveChanges();
                    }
                    // Cập nhật giao diện
                    BookingselectedRooms.Add(selectedRoom);
                    SelectedBooking.Rooms.Add(selectedRoom);
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Có lỗi khi thêm phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                }
            }
        }
    }
} 