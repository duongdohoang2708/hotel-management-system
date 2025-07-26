using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.ViewModels
{
    public class CheckoutManagementViewModel : ViewModelBase
    {
        private readonly HotelManagementDbContext _dbContext;
        public ObservableCollection<BookingDisplay> CheckedInBookings { get; set; } = new ObservableCollection<BookingDisplay>();
        public ObservableCollection<BookingDisplay> CheckedOutBookings { get; set; } = new ObservableCollection<BookingDisplay>();
        
        public BookingDisplay DisplayedBooking
        {
            get => SelectedCheckedInBooking ?? SelectedCheckedOutBooking;
        }
        
        public RoomDisplay DisplayedBookingRoom
        {
            get => SelectedCheckedInBookingRoom ?? SelectedCheckedOutBookingRoom;
        }
        
        private BookingDisplay _selectedCheckedInBooking;
        public BookingDisplay SelectedCheckedInBooking
        {
            get => _selectedCheckedInBooking;
            set
            {
                if (SetProperty(ref _selectedCheckedInBooking, value))
                {
                    OnPropertyChanged(nameof(DisplayedBooking));
                    OnPropertyChanged(nameof(CanCheckout));
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
        
        private BookingDisplay _selectedCheckedOutBooking;
        public BookingDisplay SelectedCheckedOutBooking
        {
            get => _selectedCheckedOutBooking;
            set
            {
                if (SetProperty(ref _selectedCheckedOutBooking, value))
                {
                    OnPropertyChanged(nameof(DisplayedBooking));
                }
            }
        }
        
        private RoomDisplay _selectedCheckedOutBookingRoom;
        public RoomDisplay SelectedCheckedOutBookingRoom
        {
            get => _selectedCheckedOutBookingRoom;
            set
            {
                if (SetProperty(ref _selectedCheckedOutBookingRoom, value))
                {
                    OnPropertyChanged(nameof(DisplayedBookingRoom));
                }
            }
        }
        
        public bool CanCheckout => SelectedCheckedInBooking != null;
        
        public ICommand ConfirmCheckoutCommand { get; }
        public ICommand GenerateInvoiceCommand { get; }
        public ICommand DebugCommand { get; }

        public CheckoutManagementViewModel()
        {
            _dbContext = new HotelManagementDbContext();
            LoadCheckedInBookings();
            LoadCheckedOutBookings();
            ConfirmCheckoutCommand = new RelayCommand(_ => ConfirmCheckout(), _ => CanCheckout);
            GenerateInvoiceCommand = new RelayCommand(_ => GenerateInvoice(), _ => CanCheckout);
            DebugCommand = new RelayCommand(_ => DebugBookingStatuses());
        }

        public void RefreshData()
        {
            LoadCheckedInBookings();
            LoadCheckedOutBookings();
        }

        private void DebugBookingStatuses()
        {
            var allBookings = _dbContext.Bookings
                .Include(b => b.Status)
                .Select(b => new { b.BookingId, b.StatusId, StatusName = b.Status.StatusName })
                .ToList();
            
            var statusCounts = allBookings.GroupBy(b => new { b.StatusId, b.StatusName })
                .Select(g => new { g.Key.StatusId, g.Key.StatusName, Count = g.Count() })
                .OrderBy(x => x.StatusId)
                .ToList();
            
            var message = "Booking Status Counts:\n" + 
                         string.Join("\n", statusCounts.Select(s => $"StatusId: {s.StatusId}, Name: {s.StatusName}, Count: {s.Count}"));
            
            MessageBox.Show(message, "Debug Info", MessageBoxButton.OK, MessageBoxImage.Information);
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
            public decimal? Deposit { get; set; }
            public decimal TotalAmount { get; set; }
            public ObservableCollection<RoomDisplay> Rooms { get; set; } = new ObservableCollection<RoomDisplay>();
        }

        public class RoomDisplay
        {
            public string RoomNumber { get; set; }
            public string RoomTypeName { get; set; }
            public decimal Price { get; set; }
            public ObservableCollection<ServiceUsageDisplay> ServiceUsages { get; set; } = new ObservableCollection<ServiceUsageDisplay>();
        }

        public class ServiceUsageDisplay
        {
            public string ServiceName { get; set; }
            public decimal UnitPrice { get; set; }
            public DateTime UsageTime { get; set; }
        }

        private void LoadCheckedInBookings()
        {
            CheckedInBookings.Clear();
            
            try
            {
                var bookings = _dbContext.Bookings
                    .Where(b => b.StatusId == 2)
                    .Include(b => b.Guest)
                    .Include(b => b.Status)
                    .Include(b => b.BookedRooms)
                        .ThenInclude(br => br.Room)
                            .ThenInclude(r => r.RoomType)
                    .Include(b => b.BookedRooms)
                        .ThenInclude(br => br.RoomServiceUsages)
                            .ThenInclude(rsu => rsu.Service)
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
                    b.Deposit,
                    Rooms = b.BookedRooms.Select(br => new RoomDisplay
                    {
                        RoomNumber = br.Room.RoomNumber,
                        RoomTypeName = br.Room.RoomType.TypeName,
                        Price = br.RoomPrice ?? 0,
                        ServiceUsages = new ObservableCollection<ServiceUsageDisplay>(
                            br.RoomServiceUsages.Select(su => new ServiceUsageDisplay
                            {
                                ServiceName = su.Service.ServiceName,
                                UnitPrice = su.UnitPrice ?? 0,
                                UsageTime = su.UsageTime ?? DateTime.MinValue
                            })
                        )
                    }).ToList()
                }).ToList();
                
            foreach (var b in bookings)
            {
                var totalAmount = CalculateTotalAmount(b.BookingId);
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
                    Deposit = b.Deposit,
                    TotalAmount = totalAmount,
                    Rooms = new ObservableCollection<RoomDisplay>(b.Rooms)
                };
                CheckedInBookings.Add(bookingDisplay);
            }
            
            // Debug: Show count of loaded bookings
            // MessageBox.Show($"Loaded {CheckedInBookings.Count} checked-in bookings", "Debug Info", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error loading checked-in bookings: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void LoadCheckedOutBookings()
        {
            CheckedOutBookings.Clear();
            var bookings = _dbContext.Bookings
                .Where(b => b.StatusId == 3)
                .Include(b => b.Guest)
                .Include(b => b.Status)
                .Include(b => b.BookedRooms)
                    .ThenInclude(br => br.Room)
                        .ThenInclude(r => r.RoomType)
                .Include(b => b.BookedRooms)
                    .ThenInclude(br => br.RoomServiceUsages)
                        .ThenInclude(rsu => rsu.Service)
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
                    b.Deposit,
                    Rooms = b.BookedRooms.Select(br => new RoomDisplay
                    {
                        RoomNumber = br.Room.RoomNumber,
                        RoomTypeName = br.Room.RoomType.TypeName,
                        Price = br.RoomPrice ?? 0,
                        ServiceUsages = new ObservableCollection<ServiceUsageDisplay>(
                            br.RoomServiceUsages.Select(su => new ServiceUsageDisplay
                            {
                                ServiceName = su.Service.ServiceName,
                                UnitPrice = su.UnitPrice ?? 0,
                                UsageTime = su.UsageTime ?? DateTime.MinValue
                            })
                        )
                    }).ToList()
                })
                .OrderByDescending(b => b.CheckOut) // Sắp xếp theo ngày checkout giảm dần
                .ToList();
                
            foreach (var b in bookings)
            {
                var totalAmount = CalculateTotalAmount(b.BookingId);
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
                    Deposit = b.Deposit,
                    TotalAmount = totalAmount,
                    Rooms = new ObservableCollection<RoomDisplay>(b.Rooms)
                };
                CheckedOutBookings.Add(bookingDisplay);
            }
        }

        private decimal CalculateTotalAmount(int bookingId)
        {
            var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == bookingId);
            if (booking == null) return 0;

            // Tính tổng tiền phòng
            var roomTotal = booking.BookedRooms.Sum(br => br.RoomPrice ?? 0);
            
            // Tính tổng tiền dịch vụ
            var serviceTotal = booking.BookedRooms
                .SelectMany(br => br.RoomServiceUsages)
                .Sum(rsu => rsu.UnitPrice ?? 0);
            
            return roomTotal + serviceTotal;
        }

        private void ConfirmCheckout()
        {
            if (SelectedCheckedInBooking == null) return;
            
            var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == SelectedCheckedInBooking.BookingId);
            if (booking != null)
            {
                // Cập nhật trạng thái thành đã trả phòng
                booking.StatusId = 3; // 3 = Đã trả phòng
                booking.CheckOut = DateTime.Now;
                
                // Cập nhật trạng thái phòng thành trống
                foreach (var bookedRoom in booking.BookedRooms)
                {
                    var room = _dbContext.Rooms.FirstOrDefault(r => r.RoomId == bookedRoom.RoomId);
                    if (room != null)
                    {
                        room.CleanStatus = "Chưa dọn";
                    }
                }
                
                _dbContext.SaveChanges();
                MessageBox.Show("Xác nhận trả phòng thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadCheckedInBookings();
                LoadCheckedOutBookings();
            }
        }

        private void GenerateInvoice()
        {
            if (SelectedCheckedInBooking == null) return;
            
            var booking = _dbContext.Bookings.FirstOrDefault(b => b.BookingId == SelectedCheckedInBooking.BookingId);
            if (booking == null) return;

            // Kiểm tra xem đã có hóa đơn chưa
            var existingInvoice = _dbContext.Invoices.FirstOrDefault(i => i.BookingId == booking.BookingId);
            if (existingInvoice != null)
            {
                MessageBox.Show("Hóa đơn đã được tạo cho booking này!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                return;
            }

            try
            {
                // Tạo hóa đơn mới
                var invoice = new Invoice
                {
                    BookingId = booking.BookingId,
                    StaffId = AppSession.CurrentAccount?.StaffId ?? 1, // Sử dụng staff hiện tại
                    IssueDate = DateTime.Now,
                    TotalAmount = SelectedCheckedInBooking.TotalAmount,
                    Vat = 10.0m // VAT 10%
                };

                _dbContext.Invoices.Add(invoice);
                _dbContext.SaveChanges();

                // Tạo chi tiết hóa đơn cho tiền phòng
                foreach (var bookedRoom in booking.BookedRooms)
                {
                    var invoiceDetail = new InvoiceDetail
                    {
                        InvoiceId = invoice.InvoiceId,
                        Content = $"Tiền phòng {bookedRoom.Room.RoomNumber} - {bookedRoom.Room.RoomType.TypeName}",
                        UnitPrice = bookedRoom.RoomPrice ?? 0,
                        Quantity = 1,
                        TotalPrice = bookedRoom.RoomPrice ?? 0
                    };
                    _dbContext.InvoiceDetails.Add(invoiceDetail);
                }

                // Tạo chi tiết hóa đơn cho dịch vụ
                foreach (var bookedRoom in booking.BookedRooms)
                {
                    foreach (var serviceUsage in bookedRoom.RoomServiceUsages)
                    {
                        var invoiceDetail = new InvoiceDetail
                        {
                            InvoiceId = invoice.InvoiceId,
                            Content = $"Dịch vụ {serviceUsage.Service.ServiceName}",
                            UnitPrice = serviceUsage.UnitPrice ?? 0,
                            Quantity = 1,
                            TotalPrice = serviceUsage.UnitPrice ?? 0
                        };
                        _dbContext.InvoiceDetails.Add(invoiceDetail);
                    }
                }

                _dbContext.SaveChanges();
                MessageBox.Show("Tạo hóa đơn thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi tạo hóa đơn: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        public ObservableCollection<ServiceUsageDisplay> SelectedRoomServiceUsages
        {
            get => (DisplayedBookingRoom as RoomDisplay)?.ServiceUsages ?? new ObservableCollection<ServiceUsageDisplay>();
        }
    }
} 