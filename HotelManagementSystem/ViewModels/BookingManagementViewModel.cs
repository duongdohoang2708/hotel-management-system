using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Windows.Threading;

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
                    CheckInDateError = "Ngày nhận phòng phải từ hôm nay trở đi.";
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
                        CheckOutDateError = "Ngày trả phải lớn hơn ngày nhận.";
                    }
                    else
                    {
                        CheckOutDateError = string.Empty;
                    }
                    OnPropertyChanged(nameof(SelectedCheckInText));
                    OnPropertyChanged(nameof(SelectedCheckOutText));
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
                    CheckOutDateError = "Ngày trả phải lớn hơn ngày nhận.";
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
        public RoomType SelectedRoomType { get; set; }
        public int RoomCount { get; set; }
        public int AdultCount { get; set; }
        public int ChildCount { get; set; }
        public string SpecialRequest { get; set; }

        // Danh sách
        public ObservableCollection<RoomType> RoomTypes { get; set; }
        public ObservableCollection<Room> AvailableRooms { get; set; }
        public ObservableCollection<Room> SelectedRooms { get; set; }
        public ObservableCollection<Service> Services { get; set; }
        public ObservableCollection<Service> SelectedServices { get; set; }
        public ObservableCollection<Guest> Guests { get; set; }

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
            AvailableRooms = new ObservableCollection<Room>();
            SelectedRooms = new ObservableCollection<Room>();
            Services = new ObservableCollection<Service>();
            SelectedServices = new ObservableCollection<Service>();
            Guests = new ObservableCollection<Guest>();

            // Khởi tạo timer cho tìm kiếm tự động
            _guestSearchTimer = new DispatcherTimer();
            _guestSearchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _guestSearchTimer.Tick += (s, e) =>
            {
                _guestSearchTimer.Stop();
                LoadGuests();
            };

            // Khởi tạo command (RelayCommand hoặc DelegateCommand tuỳ dự án)
            SearchRoomCommand = new RelayCommand(SearchRoom);
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
            
            LoadRoomTypes();
            LoadGuests();
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
        private void SearchRoom(object obj)
        {
            if (CheckInDate == null || CheckOutDate == null || CheckOutDate <= CheckInDate)
            {
                AvailableRooms.Clear();
                return;
            }
            var query = _dbContext.Rooms
                .Where(r => r.Status == "Còn trống");
            if (SelectedRoomType != null)
            {
                query = query.Where(r => r.RoomTypeId == SelectedRoomType.RoomTypeId);
            }
            var checkIn = CheckInDate.Value.Date;
            var checkOut = CheckOutDate.Value.Date;
            // Lấy các phòng không có booking nào giao với khoảng thời gian này
            var availableRooms = query.Where(r =>
                !r.BookedRooms.Any(br =>
                    br.Booking != null &&
                    br.Booking.CheckIn != null &&
                    br.Booking.CheckOut != null &&
                    // Kiểm tra giao khoảng
                    br.Booking.CheckIn < checkOut && br.Booking.CheckOut > checkIn
                )
            ).ToList();
            AvailableRooms.Clear();
            foreach (var room in availableRooms)
            {
                AvailableRooms.Add(room);
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
            // TODO: Hủy thao tác đặt phòng
        }

        private void PrintBooking(object obj)
        {
            // TODO: In xác nhận đặt phòng
        }
    }
} 