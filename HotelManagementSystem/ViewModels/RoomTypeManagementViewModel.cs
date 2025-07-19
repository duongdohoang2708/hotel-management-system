using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HotelManagementSystem.ViewModels
{
    public class RoomTypeManagementViewModel : ViewModelBase
    {
        public ICommand AddRoomTypeCommand { get; }
        public ICommand EditRoomTypeCommand { get; }
        public ICommand DeleteRoomTypeCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public event Action<RoomType?>? RequestAddEditRoomType;

        private RoomTypeDisplay _selectedRoomType;
        public RoomTypeDisplay SelectedRoomType
        {
            get => _selectedRoomType;
            set => SetProperty(ref _selectedRoomType, value);
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

        public class RoomTypeDisplay
        {
            public int RoomTypeId { get; set; }
            public string TypeName { get; set; }
            public string Description { get; set; }
            public decimal? BasePrice { get; set; }
        }

        private ObservableCollection<RoomTypeDisplay> _roomTypeList;
        public ObservableCollection<RoomTypeDisplay> RoomTypeList
        {
            get => _roomTypeList;
            set => SetProperty(ref _roomTypeList, value);
        }

        public RoomTypeManagementViewModel()
        {
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadRoomTypes();
            };

            AddRoomTypeCommand = new RelayCommand(_ => RequestAddEditRoomType?.Invoke(null));
            EditRoomTypeCommand = new RelayCommand(param =>
            {
                var roomType = param as RoomTypeDisplay;
                if (roomType != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbRoomType = db.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == roomType.RoomTypeId);
                        if (dbRoomType != null)
                            RequestAddEditRoomType?.Invoke(dbRoomType);
                    }
                }
            });
            DeleteRoomTypeCommand = new RelayCommand(param => DeleteRoomType(param as RoomTypeDisplay));
            SearchCommand = new RelayCommand(_ => LoadRoomTypes());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadRoomTypes();
            });
            LoadRoomTypes();
        }

        public void LoadRoomTypes()
        {
            using (var db = new HotelManagementDbContext())
            {
                var query = db.RoomTypes.AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(rt => 
                        (rt.TypeName != null && rt.TypeName.ToLower().Contains(keyword)) ||
                        (rt.Description != null && rt.Description.ToLower().Contains(keyword))
                    );
                }

                var roomTypes = query.ToList();
                RoomTypeList = new ObservableCollection<RoomTypeDisplay>(
                    roomTypes.Select(rt => new RoomTypeDisplay
                    {
                        RoomTypeId = rt.RoomTypeId,
                        TypeName = rt.TypeName ?? "",
                        Description = rt.Description ?? "",
                        BasePrice = (decimal?) rt.BasePrice,
                    })
                );
            }
        }

        private void DeleteRoomType(RoomTypeDisplay roomType)
        {
            if (roomType == null) return;
            using (var db = new HotelManagementDbContext())
            {
                // Kiểm tra ràng buộc với Room
                bool isInUse = db.Rooms.Any(r => r.RoomTypeId == roomType.RoomTypeId);
                if (isInUse)
                {
                    MessageBox.Show($"Không thể xóa loại phòng '{roomType.TypeName}' vì nó đang có trong dữ liệu phòng!", "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa loại phòng '{roomType.TypeName}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbRoomType = db.RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == roomType.RoomTypeId);
                if (dbRoomType != null)
                {
                    db.RoomTypes.Remove(dbRoomType);
                    db.SaveChanges();
                }
            }
            LoadRoomTypes();
        }
    }
} 