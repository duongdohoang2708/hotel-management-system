using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.ViewModels
{
    public class RoomManagementViewModel : ViewModelBase
    {
        public ICommand AddRoomCommand { get; }
        public ICommand EditRoomCommand { get; }
        public ICommand DeleteRoomCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }
        public ICommand AddRoomFacilityCommand { get; }
        public ICommand EditRoomFacilityCommand { get; }
        public ICommand DeleteRoomFacilityCommand { get; }

        public event Action<Room?>? RequestAddEditRoom;
        
        
        // Callback methods for direct access
        public Action<RoomManagementViewModel>? OnAddRoomFacilityRequested;
        public Action<RoomManagementViewModel, RoomFacility>? OnEditRoomFacilityRequested;

        private RoomDisplay _selectedRoomDisplay;
        public RoomDisplay SelectedRoomDisplay
        {
            get => _selectedRoomDisplay;
            set 
            { 
                SetProperty(ref _selectedRoomDisplay, value);
                LoadRoomFacilities();
                OnPropertyChanged(nameof(SelectedRoomInfo));
                OnPropertyChanged(nameof(IsRoomSelected));
            }
        }

        public Room SelectedRoom
        {
            get
            {
                if (SelectedRoomDisplay == null) return null;
                using (var db = new HotelManagementDbContext())
                {
                    return db.Rooms.Include(r => r.RoomType).FirstOrDefault(r => r.RoomId == SelectedRoomDisplay.RoomId);
                }
            }
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

        private ObservableCollection<RoomDisplay> _roomList;
        public ObservableCollection<RoomDisplay> RoomList
        {
            get => _roomList;
            set => SetProperty(ref _roomList, value);
        }

        private ObservableCollection<RoomFacilityDisplay> _roomFacilityList;
        public ObservableCollection<RoomFacilityDisplay> RoomFacilityList
        {
            get => _roomFacilityList;
            set => SetProperty(ref _roomFacilityList, value);
        }

        private RoomFacilityDisplay _selectedRoomFacility;
        public RoomFacilityDisplay SelectedRoomFacility
        {
            get => _selectedRoomFacility;
            set => SetProperty(ref _selectedRoomFacility, value);
        }

        public bool IsRoomSelected 
        { 
            get 
            {
                var result = SelectedRoomDisplay != null;
                return result;
            }
        }

        public string SelectedRoomInfo
        {
            get
            {
                if (SelectedRoomDisplay == null)
                    return "Chưa chọn phòng";
                return $"Phòng {SelectedRoomDisplay.RoomNumber} - {SelectedRoomDisplay.RoomTypeName}";
            }
        }

        public RoomManagementViewModel()
        {
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadRooms();
            };

            // Commands cho Room
            AddRoomCommand = new RelayCommand(_ => RequestAddEditRoom?.Invoke(null));
            EditRoomCommand = new RelayCommand(param =>
            {
                var room = param as RoomDisplay;
                if (room != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbRoom = db.Rooms.Include(r => r.RoomType).FirstOrDefault(r => r.RoomId == room.RoomId);
                        if (dbRoom != null)
                            RequestAddEditRoom?.Invoke(dbRoom);
                    }
                }
            });
            DeleteRoomCommand = new RelayCommand(param => DeleteRoom(param as RoomDisplay));
            SearchCommand = new RelayCommand(_ => LoadRooms());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadRooms();
            });

            // Commands cho RoomFacility
            AddRoomFacilityCommand = new RelayCommand(_ => 
            {
                if (SelectedRoomDisplay != null)
                {
                    OnAddRoomFacilityRequested?.Invoke(this);
                }
            });

            EditRoomFacilityCommand = new RelayCommand(param =>
            {
                var roomFacility = param as RoomFacilityDisplay;
                if (roomFacility != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbRoomFacility = db.RoomFacilities.Include(rf => rf.Facility).FirstOrDefault(rf => rf.RoomFacilityId == roomFacility.RoomFacilityId);
                        if (dbRoomFacility != null)
                        {
                            OnEditRoomFacilityRequested?.Invoke(this, dbRoomFacility);
                        }
                    }
                }
            });
            DeleteRoomFacilityCommand = new RelayCommand(param => DeleteRoomFacility(param as RoomFacilityDisplay));

            LoadRooms();
        }

        public void LoadRooms()
        {
            using (var db = new HotelManagementDbContext())
            {
                var query = db.Rooms.Include(r => r.RoomType).AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(r => 
                        (r.RoomNumber != null && r.RoomNumber.ToLower().Contains(keyword)) ||
                        (r.RoomType != null && r.RoomType.TypeName != null && r.RoomType.TypeName.ToLower().Contains(keyword)) ||
                        (r.Status != null && r.Status.ToLower().Contains(keyword)) ||
                        (r.CleanStatus != null && r.CleanStatus.ToLower().Contains(keyword))
                    );
                }

                var rooms = query.ToList();
                RoomList = new ObservableCollection<RoomDisplay>(
                    rooms.Select(r => new RoomDisplay
                    {
                        RoomId = r.RoomId,
                        RoomNumber = r.RoomNumber ?? "",
                        Status = r.Status ?? "",
                        CleanStatus = r.CleanStatus ?? "",
                        RoomTypeId = r.RoomTypeId,
                        RoomTypeName = r.RoomType?.TypeName ?? ""
                    })
                );
            }
        }

        public void LoadRoomFacilities()
        {
            if (SelectedRoomDisplay == null)
            {
                RoomFacilityList = new ObservableCollection<RoomFacilityDisplay>();
                return;
            }

            using (var db = new HotelManagementDbContext())
            {
                var roomFacilities = db.RoomFacilities
                    .Include(rf => rf.Facility)
                    .Where(rf => rf.RoomId == SelectedRoomDisplay.RoomId)
                    .ToList();

                RoomFacilityList = new ObservableCollection<RoomFacilityDisplay>(
                    roomFacilities.Select(rf => new RoomFacilityDisplay
                    {
                        RoomFacilityId = rf.RoomFacilityId,
                        RoomId = rf.RoomId,
                        FacilityId = rf.FacilityId,
                        FacilityName = rf.Facility?.FacilityName ?? "",
                        Description = rf.Facility?.Description ?? "",
                        Quantity = rf.Quantity
                    })
                );
            }
        }

        private void DeleteRoom(RoomDisplay room)
        {
            if (room == null) return;
            using (var db = new HotelManagementDbContext())
            {
                // Kiểm tra ràng buộc với BookedRoom
                bool isInUse = db.BookedRooms.Any(br => br.RoomId == room.RoomId);
                if (isInUse)
                {
                    MessageBox.Show($"Không thể xóa phòng '{room.RoomNumber}' vì đã có lịch sử đặt phòng!", "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa phòng '{room.RoomNumber}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbRoom = db.Rooms.FirstOrDefault(r => r.RoomId == room.RoomId);
                if (dbRoom != null)
                {
                    // Xóa các thiết bị của phòng trước
                    var roomFacilities = db.RoomFacilities.Where(rf => rf.RoomId == dbRoom.RoomId).ToList();
                    db.RoomFacilities.RemoveRange(roomFacilities);
                    
                    db.Rooms.Remove(dbRoom);
                    db.SaveChanges();
                }
            }
            LoadRooms();
        }

        private void DeleteRoomFacility(RoomFacilityDisplay roomFacility)
        {
            if (roomFacility == null) return;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa thiết bị '{roomFacility.FacilityName}' khỏi phòng?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbRoomFacility = db.RoomFacilities.FirstOrDefault(rf => rf.RoomFacilityId == roomFacility.RoomFacilityId);
                if (dbRoomFacility != null)
                {
                    db.RoomFacilities.Remove(dbRoomFacility);
                    db.SaveChanges();
                }
            }
            LoadRoomFacilities();
        }
    }
} 