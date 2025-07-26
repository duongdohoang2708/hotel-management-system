using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.ViewModels
{
    public class CheckInManagementViewModel : ViewModelBase
    {
        private readonly HotelManagementDbContext _dbContext;
        public ObservableCollection<BookingDisplay> PendingBookings { get; set; } = new ObservableCollection<BookingDisplay>();
        public BookingDisplay DisplayedBooking
        {
            get => SelectedBooking ?? SelectedCheckedInBooking;
        }
        public RoomDisplay DisplayedBookingRoom
        {
            get => SelectedBookingRoom ?? SelectedCheckedInBookingRoom;
        }
        public ObservableCollection<BookingDisplay> CheckedInBookings { get; set; } = new ObservableCollection<BookingDisplay>();
        private BookingDisplay _selectedCheckedInBooking;
        public BookingDisplay SelectedCheckedInBooking
        {
            get => _selectedCheckedInBooking;
            set
            {
                if (SetProperty(ref _selectedCheckedInBooking, value))
                {
                    OnPropertyChanged(nameof(DisplayedBooking));
                }
            }
        }
        private RoomDisplay _selectedCheckedInBookingRoom;
        public RoomDisplay SelectedCheckedInBookingRoom
        {
            get => _selectedCheckedInBookingRoom;
            set
            {
                if (SetProperty(ref _selectedCheckedInBookingRoom, value))
                {
                    OnPropertyChanged(nameof(DisplayedBookingRoom));
                }
            }
        }
        private BookingDisplay _selectedBooking;
        public BookingDisplay SelectedBooking
        {
            get => _selectedBooking;
            set
            {
                if (SetProperty(ref _selectedBooking, value))
                {
                    OnPropertyChanged(nameof(DisplayedBooking));
                }
            }
        }
        private RoomDisplay _selectedBookingRoom;
        public RoomDisplay SelectedBookingRoom
        {
            get => _selectedBookingRoom;
            set
            {
                if (SetProperty(ref _selectedBookingRoom, value))
                {
                    OnPropertyChanged(nameof(DisplayedBookingRoom));
                }
            }
        }
        public ICommand ConfirmCheckInCommand { get; }

        public CheckInManagementViewModel()
        {
            _dbContext = new HotelManagementDbContext();
            LoadPendingBookings();
            LoadCheckedInBookings();
            ConfirmCheckInCommand = new RelayCommand(_ => ConfirmCheckIn(), _ => SelectedBooking != null);
        }

        public class BookingDisplay
        {
            public int BookingId { get; set; }
            public DateTime? BookingDate { get; set; }
            public string GuestName { get; set; }
            public string GuestIdCard { get; set; }
            public string GuestAddress { get; set; }
            public int NumberOfRoom { get; set; }
            public DateTime? CheckInDate { get; set; }
            public DateTime? CheckOutDate { get; set; }
            public string StatusName { get; set; }
            public ObservableCollection<RoomDisplay> Rooms { get; set; } = new ObservableCollection<RoomDisplay>();
        }

        private void LoadPendingBookings()
        {
            PendingBookings.Clear();
            var bookings = _dbContext.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Status)
                .Include(b => b.BookedRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .Where(b => b.StatusId == 1) // StatusId = 1 is "Đã đặt" (Booked)
                .Select(b => new
                {
                    b.BookingId,
                    b.BookingDate,
                    GuestName = b.Guest.FullName,
                    GuestIdCard = b.Guest.IdCardNo,
                    GuestAddress = b.Guest.Address,
                    NumberOfRoom = b.BookedRooms.Count,
                    b.CheckIn,
                    b.CheckOut,
                    StatusName = b.Status.StatusName,
                    Rooms = b.BookedRooms.Select(br => new RoomDisplay
                    {
                        RoomNumber = br.Room.RoomNumber,
                        RoomTypeName = br.Room.RoomType.TypeName,
                        Price = br.RoomPrice ?? 0
                    }).ToList()
                }).ToList();
            foreach (var b in bookings)
            {
                var bookingDisplay = new BookingDisplay
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    GuestName = b.GuestName,
                    GuestIdCard = b.GuestIdCard,
                    GuestAddress = b.GuestAddress,
                    NumberOfRoom = b.NumberOfRoom,
                    CheckInDate = b.CheckIn,
                    CheckOutDate = b.CheckOut,
                    StatusName = b.StatusName,
                    Rooms = new ObservableCollection<RoomDisplay>(b.Rooms)
                };
                PendingBookings.Add(bookingDisplay);
            }
        }

        private void LoadCheckedInBookings()
        {
            CheckedInBookings.Clear();
            var bookings = _dbContext.Bookings
                .Include(b => b.Guest)
                .Include(b => b.Status)
                .Include(b => b.BookedRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .Where(b => b.StatusId == 2) // StatusId = 2 is "Đã nhận phòng" (Checked In)
                .Select(b => new
                {
                    b.BookingId,
                    b.BookingDate,
                    GuestName = b.Guest.FullName,
                    GuestIdCard = b.Guest.IdCardNo,
                    GuestAddress = b.Guest.Address,
                    NumberOfRoom = b.BookedRooms.Count,
                    b.CheckIn,
                    b.CheckOut,
                    StatusName = b.Status.StatusName,
                    Rooms = b.BookedRooms.Select(br => new RoomDisplay
                    {
                        RoomNumber = br.Room.RoomNumber,
                        RoomTypeName = br.Room.RoomType.TypeName,
                        Price = br.RoomPrice ?? 0
                    }).ToList()
                }).ToList();
            foreach (var b in bookings)
            {
                var bookingDisplay = new BookingDisplay
                {
                    BookingId = b.BookingId,
                    BookingDate = b.BookingDate,
                    GuestName = b.GuestName,
                    GuestIdCard = b.GuestIdCard,
                    GuestAddress = b.GuestAddress,
                    NumberOfRoom = b.NumberOfRoom,
                    CheckInDate = b.CheckIn,
                    CheckOutDate = b.CheckOut,
                    StatusName = b.StatusName,
                    Rooms = new ObservableCollection<RoomDisplay>(b.Rooms)
                };
                CheckedInBookings.Add(bookingDisplay);
            }
        }

        public class RoomDisplay
        {
            public string RoomNumber { get; set; }
            public string RoomTypeName { get; set; }
            public decimal Price { get; set; }
        }

        private void ConfirmCheckIn()
        {
            if (SelectedBooking == null) return;
            var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == SelectedBooking.BookingId);
            if (booking != null)
            {
                booking.StatusId = 2; // 2 = Đã nhận phòng
                _dbContext.SaveChanges();
                MessageBox.Show("Xác nhận nhận phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadPendingBookings();
                LoadCheckedInBookings();
            }
        }
    }
} 