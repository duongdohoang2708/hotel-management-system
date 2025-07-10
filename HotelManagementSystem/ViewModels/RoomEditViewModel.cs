using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Text.RegularExpressions;

namespace HotelManagementSystem.ViewModels
{
    public class RoomEditViewModel : ViewModelBase
    {
        public ObservableCollection<RoomType> RoomTypeList { get; }

        private string _roomNumber = string.Empty;
        public string RoomNumber
        {
            get => _roomNumber;
            set => SetProperty(ref _roomNumber, value);
        }

        private RoomType? _selectedRoomType;
        public RoomType? SelectedRoomType
        {
            get => _selectedRoomType;
            set => SetProperty(ref _selectedRoomType, value);
        }

        private int? _floor;
        public int? Floor
        {
            get => _floor;
            set => SetProperty(ref _floor, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Room>? RoomSaved;

        private readonly Room? _editingRoom;
        private readonly bool _isEditMode;
        private string _status = "Phòng trống";
        private string _cleanStatus = "Đã dọn dẹp";

        public RoomEditViewModel(Room? room = null)
        {
            using (var db = new HotelManagementDbContext())
            {
                RoomTypeList = new ObservableCollection<RoomType>(db.RoomTypes.ToList());
            }
            if (room != null)
            {
                _isEditMode = true;
                _editingRoom = room;
                RoomNumber = room.RoomNumber;
                SelectedRoomType = RoomTypeList.FirstOrDefault(rt => rt.RoomTypeId == room.RoomTypeId);
                Floor = room.Floor;
                _status = room.Status;
                _cleanStatus = room.CleanStatus;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private bool Validate()
        {
            // Validate số phòng
            if (string.IsNullOrWhiteSpace(RoomNumber))
            {
                System.Windows.MessageBox.Show("Số phòng không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (!Regex.IsMatch(RoomNumber, @"^P\d{3}$"))
            {
                System.Windows.MessageBox.Show("Số phòng phải theo định dạng Pxxx (ví dụ: P101)!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            
            // Validate số phòng không trùng
            using (var db = new HotelManagementDbContext())
            {
                var existingRoom = db.Rooms.FirstOrDefault(r => r.RoomNumber == RoomNumber);
                if (existingRoom != null && (!_isEditMode || existingRoom.RoomId != _editingRoom?.RoomId))
                {
                    System.Windows.MessageBox.Show($"Số phòng {RoomNumber} đã tồn tại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return false;
                }
            }
            
            // Validate loại phòng
            if (SelectedRoomType == null)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn loại phòng!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Validate tầng
            if (Floor == null)
            {
                System.Windows.MessageBox.Show("Tầng không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (Floor < 1 || Floor > 9)
            {
                System.Windows.MessageBox.Show("Chỉ cho phép nhập số tầng từ 1 đến 9!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void Save()
        {
            if (!Validate()) return;
            using (var db = new HotelManagementDbContext())
            {
                Room room;
                if (_isEditMode && _editingRoom != null)
                {
                    room = db.Rooms.FirstOrDefault(r => r.RoomId == _editingRoom.RoomId) ?? _editingRoom;
                    room.RoomNumber = RoomNumber;
                    room.RoomTypeId = SelectedRoomType?.RoomTypeId ?? room.RoomTypeId;
                    room.Floor = Floor;
                    // Giữ nguyên status và cleanStatus
                }
                else
                {
                    room = new Room
                    {
                        RoomNumber = RoomNumber,
                        RoomTypeId = SelectedRoomType?.RoomTypeId ?? 0,
                        Floor = Floor,
                        Status = _status,
                        CleanStatus = _cleanStatus
                    };
                    db.Rooms.Add(room);
                }
                db.SaveChanges();
                RoomSaved?.Invoke(room);
            }
            System.Windows.MessageBox.Show("Lưu phòng thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 