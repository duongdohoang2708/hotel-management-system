using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HotelManagementSystem.ViewModels
{
    public class BookingStatusManagementViewModel : ViewModelBase
    {
        public ICommand AddBookingStatusCommand { get; }
        public ICommand EditBookingStatusCommand { get; }
        public ICommand DeleteBookingStatusCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public event Action<BookingStatus?>? RequestAddEditBookingStatus;

        private BookingStatusDisplay _selectedBookingStatus;
        public BookingStatusDisplay SelectedBookingStatus
        {
            get => _selectedBookingStatus;
            set => SetProperty(ref _selectedBookingStatus, value);
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set 
            { 
                SetProperty(ref _searchKeyword, value);
                // Tìm kiếm tự động sau 500ms khi người dùng ngừng nhập
                _searchTimer?.Stop();
                _searchTimer?.Start();
            }
        }

        private DispatcherTimer _searchTimer;

        public class BookingStatusDisplay
        {
            public int StatusId { get; set; }
            public string StatusName { get; set; }
        }

        private ObservableCollection<BookingStatusDisplay> _bookingStatusList;
        public ObservableCollection<BookingStatusDisplay> BookingStatusList
        {
            get => _bookingStatusList;
            set => SetProperty(ref _bookingStatusList, value);
        }

        public BookingStatusManagementViewModel()
        {
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadBookingStatuses();
            };

            AddBookingStatusCommand = new RelayCommand(_ => RequestAddEditBookingStatus?.Invoke(null));
            EditBookingStatusCommand = new RelayCommand(param =>
            {
                var bookingStatus = param as BookingStatusDisplay;
                if (bookingStatus != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbBookingStatus = db.BookingStatuses.FirstOrDefault(bs => bs.StatusId == bookingStatus.StatusId);
                        if (dbBookingStatus != null)
                            RequestAddEditBookingStatus?.Invoke(dbBookingStatus);
                    }
                }
            });
            DeleteBookingStatusCommand = new RelayCommand(param => DeleteBookingStatus(param as BookingStatusDisplay));
            SearchCommand = new RelayCommand(_ => LoadBookingStatuses());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadBookingStatuses();
            });
            LoadBookingStatuses();
        }

        public void LoadBookingStatuses()
        {
            using (var db = new HotelManagementDbContext())
            {
                var query = db.BookingStatuses.AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(bs => 
                        (bs.StatusName != null && bs.StatusName.ToLower().Contains(keyword))
                    );
                }

                var bookingStatuses = query.ToList();
                BookingStatusList = new ObservableCollection<BookingStatusDisplay>(
                    bookingStatuses.Select(bs => new BookingStatusDisplay
                    {
                        StatusId = bs.StatusId,
                        StatusName = bs.StatusName ?? "",
                    })
                );
            }
        }

        private void DeleteBookingStatus(BookingStatusDisplay bookingStatus)
        {
            if (bookingStatus == null) return;
            using (var db = new HotelManagementDbContext())
            {
                // Kiểm tra ràng buộc với Booking
                bool isInUse = db.Bookings.Any(b => b.StatusId == bookingStatus.StatusId);
                if (isInUse)
                {
                    MessageBox.Show($"Không thể xóa trạng thái '{bookingStatus.StatusName}' vì nó đang được sử dụng trong dữ liệu đặt phòng!", "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa trạng thái '{bookingStatus.StatusName}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbBookingStatus = db.BookingStatuses.FirstOrDefault(bs => bs.StatusId == bookingStatus.StatusId);
                if (dbBookingStatus != null)
                {
                    db.BookingStatuses.Remove(dbBookingStatus);
                    db.SaveChanges();
                }
            }
            LoadBookingStatuses();
        }
    }
} 