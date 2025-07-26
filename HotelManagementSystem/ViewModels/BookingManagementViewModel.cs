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

                if (newValue != null && false) //newValue < today)
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
                    OnPropertyChanged(nameof(CanConfirmBookingProperty));
                    OnPropertyChanged(nameof(CanCancelBookingProperty));
                    // Chỉ gọi SearchRoom nếu không phải đang cập nhật từ SelectedBooking
                    if (!_isUpdatingFromSelectedBooking)
                    {
                        SearchRoom();
                    }
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
                    OnPropertyChanged(nameof(CanConfirmBookingProperty));
                    OnPropertyChanged(nameof(CanCancelBookingProperty));
                    // Chỉ gọi SearchRoom nếu không phải đang cập nhật từ SelectedBooking
                    if (!_isUpdatingFromSelectedBooking)
                    {
                        SearchRoom();
                    }
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
                if (CheckInDate == null) //|| CheckInDate < DateTime.Today)
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
                    // Chỉ gọi SearchRoom nếu không phải đang cập nhật từ SelectedBooking
                    if (!_isUpdatingFromSelectedBooking)
                    {
                        SearchRoom();
                    }
                }
            }
        }

        // Danh sách
        public ObservableCollection<RoomType> RoomTypes { get; set; }
        public ObservableCollection<RoomDisplayForRoomManagement> AvailableRooms { get; set; }
        public ObservableCollection<RoomDisplayForRoomManagement> SelectedRoomsForNewBooking { get; set; }
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
                    // Chỉ xử lý nếu không phải đang cập nhật từ LoadBookings
                    if (!_isUpdatingFromLoadBookings)
                    {
                        BookedRooms.Clear();
                        // Reset SelectedAvailableRoom khi booking thay đổi
                        SelectedAvailableRoom = null;
                        // Load BookedRooms từ database
                        if (value != null)
                        {
                            LoadBookedRooms(value.BookingId, false); // Không cập nhật AvailableRooms ở đây
                            // Cập nhật CheckInDate và CheckOutDate để hiển thị thời gian của booking
                            _isUpdatingFromSelectedBooking = true;
                            CheckInDate = value.CheckInDate;
                            CheckOutDate = value.CheckOutDate;
                            // Reset các filter khi chọn booking
                            SelectedRoomType = null;
                            FilterMaxPrice = string.Empty;
                            _isUpdatingFromSelectedBooking = false;
                            // Cập nhật danh sách phòng có sẵn sau khi đã cập nhật tất cả
                            SearchRoom();
                        }
                        else
                        {
                            // Reset CheckInDate và CheckOutDate khi không có booking nào được chọn
                            _isUpdatingFromSelectedBooking = true;
                            CheckInDate = null;
                            CheckOutDate = null;
                            // Reset các filter khi không có booking nào được chọn
                            SelectedRoomType = null;
                            FilterMaxPrice = string.Empty;
                            _isUpdatingFromSelectedBooking = false;
                            // Cập nhật danh sách phòng có sẵn khi không có booking nào được chọn
                            SearchRoom();
                        }
                    }
                                         // Cập nhật trạng thái enable/disable các nút lệnh
                     if (value == null)
                     {
                         CanEditBooking = false;
                         CanDeleteBooking = false;
                         CanCancelBooking = false;
                     }
                     else
                     {
                         int status = value.StatusId ?? 0;
                         
                         // Quy tắc điều khiển nút Sửa đặt phòng - chỉ active với StatusID = 1 hoặc 2
                         CanEditBooking = (status == 1 || status == 2);
                         
                         // Quy tắc điều khiển nút Xóa đặt phòng - chỉ active với StatusID = 4 hoặc 5
                         CanDeleteBooking = (status == 4 || status == 5);
                         
                         // Quy tắc điều khiển nút Hủy đặt phòng dựa trên trạng thái
                         if (status == 1) //Đặt phòng
                         {
                             CanCancelBooking = true;
                         }
                         else if (status == 2 || status == 3) //Đã nhận phòng hoặc đã trả phòng
                         {
                             CanCancelBooking = false;
                         }
                         else if (status == 4) //Không đến nhận phòng
                         {
                             CanCancelBooking = false;
                         }
                         else if (status == 5) // Đã hủy phòng
                         {
                             CanCancelBooking = true;
                         }
                         else if (status > 5) //Các trạng thái khác (ví dụ: đã thanh toán, đã hoàn thành)
                         {
                             CanCancelBooking = false;
                         }
                         else
                         {
                             CanCancelBooking = false;
                         }
                     }
                    // Trigger property change notifications for room management buttons
                    OnPropertyChanged(nameof(CanAddBookingRoom));
                    OnPropertyChanged(nameof(CanDeleteBookingRoom));
                }
            }
        }

        // Danh sách các booking đang chờ
        public ObservableCollection<BookingDisplay> Bookings { get; set; } = new ObservableCollection<BookingDisplay>();
        public ObservableCollection<BookedRoom> BookedRooms { get; set; } = new ObservableCollection<BookedRoom>();

        // Thuộc tính cho dòng được chọn trong BookingRoomsDataGrid
        private BookedRoom _selectedBookedRoom;
        public BookedRoom SelectedBookedRoom
        {
            get => _selectedBookedRoom;
            set
            {
                if (SetProperty(ref _selectedBookedRoom, value))
                {
                    OnPropertyChanged(nameof(CanDeleteBookingRoom));
                }
            }
        }
        // Thuộc tính bool để enable/disable nút Xóa phòng
        public bool CanDeleteBookingRoom => SelectedBookedRoom != null && 
                                           SelectedBooking != null && 
                                           (SelectedBooking.StatusId == 1 || SelectedBooking.StatusId == 2);

        // Thuộc tính bool để enable/disable nút Thêm phòng
        public bool CanAddBookingRoom => SelectedBooking != null && 
                                        (SelectedBooking.StatusId == 1 || SelectedBooking.StatusId == 2) &&
                                        SelectedAvailableRoom != null;

        // Property để theo dõi phòng được chọn từ RoomsDataGrid
        private RoomDisplayForRoomManagement _selectedAvailableRoom;
        public RoomDisplayForRoomManagement SelectedAvailableRoom
        {
            get => _selectedAvailableRoom;
            set
            {
                if (SetProperty(ref _selectedAvailableRoom, value))
                {
                    OnPropertyChanged(nameof(CanAddBookingRoom));
                }
            }
        }

        // Thuộc tính bool để enable/disable nút Hủy (tương tự như CanConfirmBookingProperty)
        public bool CanCancelBookingProperty => CanCancelBookingInternal();

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
                    OnPropertyChanged(nameof(CanConfirmBookingProperty));
                    OnPropertyChanged(nameof(CanCancelBookingProperty));
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
            set
            {
                if (SetProperty(ref _filterMaxPrice, value))
                {
                    // Chỉ gọi SearchRoom nếu không phải đang cập nhật từ SelectedBooking
                    if (!_isUpdatingFromSelectedBooking)
                    {
                        SearchRoom();
                    }
                }
            }
        }
        public ICommand FilterMaxPriceEnterCommand { get; }

        // Command
        public ICommand SearchRoomCommand { get; }
        public ICommand ResetCommand { get; }
        public ICommand ConfirmBookingCommand { get; }
        public ICommand CancelBookingCommand { get; }
        
        public ICommand AddRoomToNewBookingCommand { get; }
        public ICommand DeleteBookingCommand { get; }
        public ICommand CancelSelectedBookingCommand { get; }
        public ICommand EditBookingCommand { get; }
        public ICommand DeleteRoomCommand { get; }
        public ICommand AddRoomCommand { get; }

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
        private bool _canCancelBooking;
        public bool CanCancelBooking
        {
            get => _canCancelBooking;
            set => SetProperty(ref _canCancelBooking, value);
        }

        // DbContext
        private readonly HotelManagementDbContext _dbContext;
        
        // Flag để kiểm soát việc gọi SearchRoom khi thay đổi CheckInDate/CheckOutDate
        private bool _isUpdatingFromSelectedBooking = false;
        
        // Flag để kiểm soát việc gọi SearchRoom khi LoadBookings
        private bool _isUpdatingFromLoadBookings = false;

        public decimal TotalRoomPricePerDay
        {
            get
            {
                if (SelectedRoomsForNewBooking == null) return 0;
                return SelectedRoomsForNewBooking.Sum(r => r.Price);
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
                    // Chỉ gọi LoadBookings nếu không phải đang cập nhật từ ConfirmBooking
                    if (!_isUpdatingFromLoadBookings)
                    {
                        LoadBookings();
                    }
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
            SelectedRoomsForNewBooking = new ObservableCollection<RoomDisplayForRoomManagement>();
            Guests = new ObservableCollection<Guest>();
            Bookings = new ObservableCollection<BookingDisplay>();
            BookedRooms = new ObservableCollection<BookedRoom>();
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
            ConfirmBookingCommand = new RelayCommand(ConfirmBooking, _ => CanConfirmBookingInternal());
            CancelBookingCommand = new RelayCommand(CancelBooking, _ => CanCancelBookingInternal());
            
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
            AddRoomToNewBookingCommand = new RelayCommand(param => AddRoomToNewBooking(param), _ => true);
            SelectedRoomsForNewBooking.CollectionChanged += (s, e) =>
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
                OnPropertyChanged(nameof(CanConfirmBookingProperty));
                OnPropertyChanged(nameof(CanCancelBookingProperty));
            };
            _isUpdatingFromLoadBookings = true;
            LoadBookings();
            _isUpdatingFromLoadBookings = false;
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
            CancelSelectedBookingCommand = new RelayCommand(CancelSelectedBooking, _ => CanCancelSelectedBooking());
            EditBookingCommand = new RelayCommand(_ => EditBooking(), _ => SelectedBooking != null);
            DeleteRoomCommand = new RelayCommand(_ => DeleteRoom(), _ => CanDeleteBookingRoom);
            AddRoomCommand = new RelayCommand(_ => AddRoom(), _ => CanAddBookingRoom);
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
            DateTime? checkIn = null;
            DateTime? checkOut = null;
            
            // Xác định thời gian tìm kiếm dựa trên context
            if (SelectedBooking != null)
            {
                // Nếu đang thêm phòng vào booking hiện tại, sử dụng thời gian của booking
                checkIn = SelectedBooking.CheckInDate;
                checkOut = SelectedBooking.CheckOutDate;
            }
            else
            {
                // Nếu đang tạo booking mới, sử dụng thời gian từ DatePicker
                if (CheckInDate == null || CheckOutDate == null || CheckOutDate <= CheckInDate)
                {
                    AvailableRooms.Clear();
                    return;
                }
                checkIn = CheckInDate.Value.Date;
                checkOut = CheckOutDate.Value.Date;
            }
            
            if (checkIn == null || checkOut == null || checkOut <= checkIn)
            {
                AvailableRooms.Clear();
                return;
            }

            // Lấy tất cả bookings để kiểm tra trạng thái phòng
            var allBookings = _dbContext.Bookings
                .Include(b => b.BookedRooms)
                .ToList();

            var query = _dbContext.Rooms
                .Include(r => r.RoomType)
                .AsEnumerable() // Chuyển sang LINQ to Objects để có thể gọi method
                .Where(r => r.GetStatusByDuration(checkIn.Value, checkOut.Value, allBookings) == "Trống");
            
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
            
            var availableRooms = query.ToList();

            AvailableRooms.Clear();
            foreach (var room in availableRooms)
            {
                // Nếu đang thêm phòng vào booking hiện tại, kiểm tra phòng đã có trong booking chưa
                if (SelectedBooking != null)
                {
                    bool alreadyInBooking = BookedRooms.Any(br => br.RoomId == room.RoomId);
                    if (alreadyInBooking)
                    {
                        // Debug: Log phòng bị bỏ qua vì đã có trong booking
                        continue; // Bỏ qua phòng đã có trong booking
                    }
                }
                else
                {
                    // Nếu đang tạo booking mới, kiểm tra phòng đã được chọn chưa
                    if (SelectedRoomsForNewBooking.Any(r => r.RoomId == room.RoomId))
                        continue; // Bỏ qua phòng đã chọn
                }
                
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

       
        private void Reset(object obj)
        {
            // TODO: Đặt lại điều kiện tìm kiếm
        }

        private bool CanConfirmBookingInternal()
        {
            return SelectedGuest != null && 
                   SelectedRoomsForNewBooking != null && 
                   SelectedRoomsForNewBooking.Count > 0 &&
                   CheckInDate != null && 
                   CheckOutDate != null && 
                   CheckOutDate > CheckInDate;
        }

        private bool CanCancelBookingInternal()
        {
            return SelectedGuest != null && 
                   SelectedRoomsForNewBooking != null && 
                   SelectedRoomsForNewBooking.Count > 0 &&
                   CheckInDate != null && 
                   CheckOutDate != null && 
                   CheckOutDate > CheckInDate;
        }

        public bool CanConfirmBookingProperty => CanConfirmBookingInternal();

        private void ConfirmBooking(object obj)
        {
            if (SelectedGuest == null || SelectedRoomsForNewBooking == null || SelectedRoomsForNewBooking.Count == 0)
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
                        StatusId = 1, // Đặt phòng
                        StaffId = AppSession.CurrentAccount.StaffId 
                    };
                    db.Bookings.Add(booking);
                    db.SaveChanges();
                    // Thêm các booked room
                    foreach (var room in SelectedRoomsForNewBooking)
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
                _isUpdatingFromLoadBookings = true;
                LoadBookings();
                // Lọc theo tên khách vừa đặt
                BookingSearchKeyword = SelectedGuest.FullName;
                LoadBookings();
                // Chọn dòng vừa thêm
                Application.Current.Dispatcher.InvokeAsync(() =>
                {
                    SelectedBooking = Bookings.FirstOrDefault(b => b.GuestId == SelectedGuest.GuestId && b.CheckInDate == CheckInDate && b.CheckOutDate == CheckOutDate);
                }, System.Windows.Threading.DispatcherPriority.Background);
                _isUpdatingFromLoadBookings = false;
                // Reset danh sách phòng đã chọn
                SelectedRoomsForNewBooking.Clear();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelBooking(object obj)
        {
            SelectedRoomsForNewBooking.Clear();      // Xóa toàn bộ phòng đã chọn
            SelectedGuest = null;       // Xóa thông tin khách hàng đang chọn
            SearchRoom();
        }

       

        // Load  mọi booking
        private void LoadBookings()
        {
            // Tự động cập nhật trạng thái các booking đã đặt nhưng đã quá ngày check-in
            var currentDate = DateTime.Today;
            var expiredBookings = _dbContext.Bookings
                .Where(b => b.StatusId == 1 && b.CheckIn < currentDate)
                .ToList();
            
            if (expiredBookings.Any())
            {
                foreach (var expiredBooking in expiredBookings)
                {
                    expiredBooking.StatusId = 5; // Đã hủy phòng
                }
                _dbContext.SaveChanges();
            }

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

        private void LoadBookedRooms(int bookingId, bool updateAvailableRooms = true)
        {
            BookedRooms.Clear();
            var bookedRooms = _dbContext.BookedRooms
                .Include(br => br.Room)
                .Include(br => br.Room.RoomType)
                .Where(br => br.BookingId == bookingId)
                .ToList();
            
            foreach (var bookedRoom in bookedRooms)
            {
                BookedRooms.Add(bookedRoom);
            }
            
            // Cập nhật danh sách phòng có sẵn sau khi load BookedRooms
            if (updateAvailableRooms)
            {
                SearchRoom();
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
                SearchRoom();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi khi hủy đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                _isUpdatingFromLoadBookings = true;
                LoadBookings();
                SelectedBooking = Bookings.FirstOrDefault(b => b.BookingId == selectedBookingId);
                _isUpdatingFromLoadBookings = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi xảy ra khi hủy đặt phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
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
                _isUpdatingFromLoadBookings = true;
                LoadBookings();
                SelectedBooking = Bookings.FirstOrDefault(b => b.BookingId == booking.BookingId);
                _isUpdatingFromLoadBookings = false;
            }
        }

        private void DeleteRoom()
        {
            if (SelectedBooking == null || SelectedBookedRoom == null) return;
            string roomInfo = $"Phòng {SelectedBookedRoom.Room.RoomNumber} - {SelectedBookedRoom.Room.RoomType.TypeName}";
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa {roomInfo} khỏi đơn đặt phòng này không?", "Xác nhận xóa phòng", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;
            try
            {
                using (var db = new HotelManagementDbContext())
                {
                    var bookedRoom = db.BookedRooms.FirstOrDefault(br => br.BookingId == SelectedBooking.BookingId && br.RoomId == SelectedBookedRoom.RoomId);
                    if (bookedRoom != null)
                    {
                        db.BookedRooms.Remove(bookedRoom);
                        db.SaveChanges();
                    }
                }
                // Lưu thông tin cần thiết trước khi cập nhật
                var currentBookingId = SelectedBooking.BookingId;
                var deletedRoomId = SelectedBookedRoom.RoomId;
                
                // Xóa khỏi danh sách hiển thị
                BookedRooms.Remove(SelectedBookedRoom);
                
                // Refresh context để đảm bảo dữ liệu được cập nhật
                _dbContext.ChangeTracker.Clear();
                
                // Cập nhật lại danh sách BookedRooms từ database
                LoadBookedRooms(currentBookingId, false); // Không cần gọi SearchRoom() vì sẽ được gọi sau

                SelectedBookedRoom = null;
                
                // Cập nhật danh sách phòng có sẵn
                SearchRoom();
                
                // Debug: Kiểm tra xem phòng đã được thêm vào AvailableRooms chưa
                var addedRoom = AvailableRooms.FirstOrDefault(r => r.RoomId == deletedRoomId);
                if (addedRoom != null)
                {
                    // Phòng đã được thêm vào AvailableRooms thành công
                }
                else
                {
                    // Phòng chưa được thêm vào AvailableRooms - có thể có vấn đề với logic SearchRoom
                    // Thử thêm phòng thủ công nếu cần
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi khi xóa phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void AddRoom()
        {
            if (SelectedBooking == null || SelectedAvailableRoom == null) return;
            
            try
            {
                using (var db = new HotelManagementDbContext())
                {
                    // Kiểm tra xem phòng đã có trong booking chưa
                    var existingBookedRoom = db.BookedRooms.FirstOrDefault(br => 
                        br.BookingId == SelectedBooking.BookingId && br.RoomId == SelectedAvailableRoom.RoomId);
                    
                    if (existingBookedRoom != null)
                    {
                        MessageBox.Show($"Phòng {SelectedAvailableRoom.RoomNumber} đã có trong booking này!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Warning);
                        return;
                    }
                    
                    // Thêm phòng vào booking
                    var bookedRoom = new BookedRoom
                    {
                        BookingId = SelectedBooking.BookingId,
                        RoomId = SelectedAvailableRoom.RoomId,
                        RoomPrice = SelectedAvailableRoom.Price
                    };
                    db.BookedRooms.Add(bookedRoom);
                    db.SaveChanges();
                }
                
                // Refresh context để đảm bảo dữ liệu được cập nhật
                _dbContext.ChangeTracker.Clear();
                
                // Lưu BookingId trước khi cập nhật
                var currentBookingId = SelectedBooking.BookingId;
                
                // Cập nhật lại danh sách BookedRooms
                LoadBookedRooms(currentBookingId, false); // Không cần gọi SearchRoom() vì sẽ được gọi sau
                // Cập nhật lại danh sách Bookings để cập nhật số lượng phòng
                _isUpdatingFromLoadBookings = true;
                LoadBookings();
                // Giữ lại dòng vừa thêm phòng
                SelectedBooking = Bookings.FirstOrDefault(b => b.BookingId == currentBookingId);
                _isUpdatingFromLoadBookings = false;
                
                // Kiểm tra nếu SelectedBooking bị null sau khi LoadBookings
                if (SelectedBooking == null)
                {
                    MessageBox.Show("Không thể tìm thấy booking sau khi cập nhật!", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
                
                // Lưu thông tin phòng trước khi reset
                var roomId = SelectedAvailableRoom.RoomId;
                var roomNumber = SelectedAvailableRoom.RoomNumber;
                
                // Reset selection
                SelectedAvailableRoom = null;
                
                // Xóa phòng khỏi AvailableRooms để tránh thêm lại
                var roomToRemove = AvailableRooms.FirstOrDefault(r => r.RoomId == roomId);
                if (roomToRemove != null)
                {
                    AvailableRooms.Remove(roomToRemove);
                }
                
                // Cập nhật danh sách phòng có sẵn
                SearchRoom();
                
                MessageBox.Show($"Đã thêm phòng {roomNumber} vào booking!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi khi thêm phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
        
        private ObservableCollection<RoomDisplay> GetAvailableRoomsForBooking(DateTime? checkIn, DateTime? checkOut)
        {
            var availableRooms = new ObservableCollection<RoomDisplay>();
            
            if (checkIn == null || checkOut == null || checkOut <= checkIn)
                return availableRooms;
            
            var checkInDate = checkIn.Value.Date;
            var checkOutDate = checkOut.Value.Date;
            
            // Lấy tất cả bookings để kiểm tra trạng thái phòng
            var allBookings = _dbContext.Bookings
                .Include(b => b.BookedRooms)
                .ToList();
            
            var query = _dbContext.Rooms
                .Include(r => r.RoomType)
                .AsEnumerable()
                .Where(r => r.GetStatusByDuration(checkInDate, checkOutDate, allBookings) == "Trống");
            
            var rooms = query.ToList();
            
            foreach (var room in rooms)
            {
                // Kiểm tra xem phòng đã có trong booking hiện tại chưa
                bool alreadyInBooking = BookedRooms.Any(br => br.RoomId == room.RoomId);
                if (!alreadyInBooking)
                {
                    var roomDisplay = new RoomDisplay
                    {
                        RoomId = room.RoomId,
                        RoomNumber = room.RoomNumber,
                        RoomTypeName = room.RoomType.TypeName,
                        Price = room.RoomType.BasePrice ?? 0
                    };
                    availableRooms.Add(roomDisplay);
                }
            }
            
            return availableRooms;
        }

        private void AddRoomToNewBooking(object parameter)
        {
            // Lấy phòng từ parameter
            var roomDisplay = parameter as RoomDisplayForRoomManagement;
            if (roomDisplay == null) return;
            
            // Kiểm tra xem phòng đã được thêm vào SelectedRoomsForNewBooking chưa
            if (SelectedRoomsForNewBooking.Any(r => r.RoomId == roomDisplay.RoomId))
            {
                MessageBox.Show($"Phòng {roomDisplay.RoomNumber} đã được thêm vào danh sách!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }
            
            // Kiểm tra ngày nhận và ngày trả
            var checkIn = CheckInDate;
            var checkOut = CheckOutDate;
            if (checkIn == null || checkOut == null)
            {
                MessageBox.Show("Vui lòng chọn ngày nhận và ngày trả cho booking này!", "Thiếu thông tin", MessageBoxButton.OK, MessageBoxImage.Warning);
                return;
            }
            
            // Nếu có SelectedBooking, kiểm tra xem phòng đã có trong booking hiện tại chưa
            if (SelectedBooking != null)
            {
                bool alreadyInBooking = SelectedBooking.Rooms.Any(r => r.RoomId == roomDisplay.RoomId);
                if (alreadyInBooking)
                {
                    MessageBox.Show($"Phòng {roomDisplay.RoomNumber} đã có trong booking này!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
            }
            
            try
            {
                // Thêm phòng vào SelectedRoomsForNewBooking
                var selectedRoom = new RoomDisplayForRoomManagement
                {
                    RoomId = roomDisplay.RoomId,
                    RoomNumber = roomDisplay.RoomNumber,
                    RoomType = roomDisplay.RoomType,
                    Price = roomDisplay.Price,
                    IsSelected = false
                };
                
                SelectedRoomsForNewBooking.Add(selectedRoom);
                

                
                // Xóa phòng khỏi AvailableRooms để tránh thêm lại
                var roomToRemove = AvailableRooms.FirstOrDefault(r => r.RoomId == roomDisplay.RoomId);
                if (roomToRemove != null)
                {
                    AvailableRooms.Remove(roomToRemove);
                }
                
                MessageBox.Show($"Đã thêm phòng {roomDisplay.RoomNumber} vào booking!", "Thành công", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Có lỗi khi thêm phòng: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 