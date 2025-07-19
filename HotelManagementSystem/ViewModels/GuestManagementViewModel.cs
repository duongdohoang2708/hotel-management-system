using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HotelManagementSystem.ViewModels
{
    public class GuestManagementViewModel : ViewModelBase
    {
        public ICommand AddGuestCommand { get; }
        public ICommand EditGuestCommand { get; }
        public ICommand DeleteGuestCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public event Action<Guest?>? RequestAddEditGuest;

        private GuestDisplay _selectedGuest;
        public GuestDisplay SelectedGuest
        {
            get => _selectedGuest;
            set => SetProperty(ref _selectedGuest, value);
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

        public class GuestDisplay
        {
            public int GuestId { get; set; }
            public string FullName { get; set; }
            public string Address { get; set; }
            public string Phone { get; set; }
            public string IdCardNo { get; set; }
        }

        private ObservableCollection<GuestDisplay> _guestList;
        public ObservableCollection<GuestDisplay> GuestList
        {
            get => _guestList;
            set => SetProperty(ref _guestList, value);
        }

        public GuestManagementViewModel()
        {
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadGuests();
            };

            AddGuestCommand = new RelayCommand(_ => RequestAddEditGuest?.Invoke(null));
            EditGuestCommand = new RelayCommand(param =>
            {
                var guest = param as GuestDisplay;
                if (guest != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbGuest = db.Guests.FirstOrDefault(g => g.GuestId == guest.GuestId);
                        if (dbGuest != null)
                            RequestAddEditGuest?.Invoke(dbGuest);
                    }
                }
            });
            DeleteGuestCommand = new RelayCommand(param => DeleteGuest(param as GuestDisplay));
            SearchCommand = new RelayCommand(_ => LoadGuests());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadGuests();
            });
            LoadGuests();
        }

        public void LoadGuests()
        {
            using (var db = new HotelManagementDbContext())
            {
                var query = db.Guests.AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(g => 
                        (g.FullName != null && g.FullName.ToLower().Contains(keyword)) ||
                        (g.Phone != null && g.Phone.ToLower().Contains(keyword)) ||
                        (g.IdCardNo != null && g.IdCardNo.ToLower().Contains(keyword))
                    );
                }

                var guests = query.ToList();
                GuestList = new ObservableCollection<GuestDisplay>(
                    guests.Select(g => new GuestDisplay
                    {
                        GuestId = g.GuestId,
                        FullName = g.FullName ?? "",
                        Address = g.Address ?? "",
                        Phone = g.Phone ?? "",
                        IdCardNo = g.IdCardNo ?? "",
                    })
                );
            }
        }

        private void DeleteGuest(GuestDisplay guest)
        {
            if (guest == null) return;
            using (var db = new HotelManagementDbContext())
            {
                // Kiểm tra ràng buộc với Booking
                bool isInUse = db.Bookings.Any(b => b.GuestId == guest.GuestId);
                if (isInUse)
                {
                    MessageBox.Show($"Không thể xóa khách hàng '{guest.FullName}' vì đã có lịch sử đặt phòng!", "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa khách hàng '{guest.FullName}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbGuest = db.Guests.FirstOrDefault(g => g.GuestId == guest.GuestId);
                if (dbGuest != null)
                {
                    db.Guests.Remove(dbGuest);
                    db.SaveChanges();
                }
            }
            LoadGuests();
        }
    }
} 