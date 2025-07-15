using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class RoomFacilityEditViewModel : ViewModelBase
    {
        public ObservableCollection<Room> RoomList { get; }
        public ObservableCollection<Facility> FacilityList { get; }

        private Room? _selectedRoom;
        public Room? SelectedRoom
        {
            get => _selectedRoom;
            set => SetProperty(ref _selectedRoom, value);
        }

        private Facility? _selectedFacility;
        public Facility? SelectedFacility
        {
            get => _selectedFacility;
            set => SetProperty(ref _selectedFacility, value);
        }

        private int? _quantity;
        public int? Quantity
        {
            get => _quantity;
            set => SetProperty(ref _quantity, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<RoomFacility>? RoomFacilitySaved;

        private readonly RoomFacility? _editingRoomFacility;
        private readonly bool _isEditMode;

        public RoomFacilityEditViewModel(RoomFacility? roomFacility = null)
        {
            using (var db = new HotelManagementDbContext())
            {
                RoomList = new ObservableCollection<Room>(db.Rooms.ToList());
                FacilityList = new ObservableCollection<Facility>(db.Facilities.ToList());
            }
            if (roomFacility != null)
            {
                _isEditMode = true;
                _editingRoomFacility = roomFacility;
                SelectedRoom = RoomList.FirstOrDefault(r => r.RoomId == roomFacility.RoomId);
                SelectedFacility = FacilityList.FirstOrDefault(a => a.FacilityId == roomFacility.FacilityId);
                Quantity = roomFacility.Quantity;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private bool Validate()
        {
            // Validate phòng
            if (SelectedRoom == null)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn phòng!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Validate tiện nghi
            if (SelectedFacility == null)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn tiện nghi!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Validate số lượng
            if (Quantity == null)
            {
                System.Windows.MessageBox.Show("Vui lòng nhập số lượng!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (Quantity <= 0)
            {
                System.Windows.MessageBox.Show("Số lượng phải là số nguyên dương!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Không cho phép trùng phòng-tiện nghi
            using (var db = new HotelManagementDbContext())
            {
                var existing = db.RoomFacilities.FirstOrDefault(ra => ra.RoomId == SelectedRoom.RoomId && ra.FacilityId == SelectedFacility.FacilityId);
                if (existing != null && (!_isEditMode || existing.RoomId != _editingRoomFacility?.RoomId || existing.FacilityId != _editingRoomFacility?.FacilityId))
                {
                    System.Windows.MessageBox.Show($"Phòng {SelectedRoom.RoomNumber} đã có tiện nghi '{SelectedFacility.FacilityName}'!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return false;
                }
            }
            return true;
        }

        private void Save()
        {
            if (!Validate()) return;
            using (var db = new HotelManagementDbContext())
            {
                RoomFacility roomFacility;
                if (_isEditMode && _editingRoomFacility != null)
                {
                    roomFacility = db.RoomFacilities.FirstOrDefault(ra => ra.RoomId == _editingRoomFacility.RoomId && ra.FacilityId == _editingRoomFacility.FacilityId) ?? _editingRoomFacility;
                    roomFacility.RoomId = SelectedRoom?.RoomId ?? roomFacility.RoomId;
                    roomFacility.FacilityId = SelectedFacility?.FacilityId ?? roomFacility.FacilityId;
                    roomFacility.Quantity = Quantity ?? roomFacility.Quantity;
                }
                else
                {
                    roomFacility = new RoomFacility
                    {
                        RoomId = SelectedRoom?.RoomId ?? 0,
                        FacilityId = SelectedFacility?.FacilityId ?? 0,
                        Quantity = Quantity ?? 1
                    };
                    db.RoomFacilities.Add(roomFacility);
                }
                db.SaveChanges();
                RoomFacilitySaved?.Invoke(roomFacility);
            }
            System.Windows.MessageBox.Show("Lưu chi tiết tiện nghi thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 