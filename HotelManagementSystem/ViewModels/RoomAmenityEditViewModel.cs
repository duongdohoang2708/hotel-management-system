using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class RoomAmenityEditViewModel : ViewModelBase
    {
        public ObservableCollection<Room> RoomList { get; }
        public ObservableCollection<Amenity> AmenityList { get; }

        private Room? _selectedRoom;
        public Room? SelectedRoom
        {
            get => _selectedRoom;
            set => SetProperty(ref _selectedRoom, value);
        }

        private Amenity? _selectedAmenity;
        public Amenity? SelectedAmenity
        {
            get => _selectedAmenity;
            set => SetProperty(ref _selectedAmenity, value);
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
        public event Action<RoomAmenity>? RoomAmenitySaved;

        private readonly RoomAmenity? _editingRoomAmenity;
        private readonly bool _isEditMode;

        public RoomAmenityEditViewModel(RoomAmenity? roomAmenity = null)
        {
            using (var db = new HotelManagementDbContext())
            {
                RoomList = new ObservableCollection<Room>(db.Rooms.ToList());
                AmenityList = new ObservableCollection<Amenity>(db.Amenities.ToList());
            }
            if (roomAmenity != null)
            {
                _isEditMode = true;
                _editingRoomAmenity = roomAmenity;
                SelectedRoom = RoomList.FirstOrDefault(r => r.RoomId == roomAmenity.RoomId);
                SelectedAmenity = AmenityList.FirstOrDefault(a => a.AmenityId == roomAmenity.AmenityId);
                Quantity = roomAmenity.Quantity;
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
            if (SelectedAmenity == null)
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
                var existing = db.RoomAmenities.FirstOrDefault(ra => ra.RoomId == SelectedRoom.RoomId && ra.AmenityId == SelectedAmenity.AmenityId);
                if (existing != null && (!_isEditMode || existing.RoomId != _editingRoomAmenity?.RoomId || existing.AmenityId != _editingRoomAmenity?.AmenityId))
                {
                    System.Windows.MessageBox.Show($"Phòng {SelectedRoom.RoomNumber} đã có tiện nghi '{SelectedAmenity.AmenityName}'!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                RoomAmenity roomAmenity;
                if (_isEditMode && _editingRoomAmenity != null)
                {
                    roomAmenity = db.RoomAmenities.FirstOrDefault(ra => ra.RoomId == _editingRoomAmenity.RoomId && ra.AmenityId == _editingRoomAmenity.AmenityId) ?? _editingRoomAmenity;
                    roomAmenity.RoomId = SelectedRoom?.RoomId ?? roomAmenity.RoomId;
                    roomAmenity.AmenityId = SelectedAmenity?.AmenityId ?? roomAmenity.AmenityId;
                    roomAmenity.Quantity = Quantity ?? roomAmenity.Quantity;
                }
                else
                {
                    roomAmenity = new RoomAmenity
                    {
                        RoomId = SelectedRoom?.RoomId ?? 0,
                        AmenityId = SelectedAmenity?.AmenityId ?? 0,
                        Quantity = Quantity ?? 1
                    };
                    db.RoomAmenities.Add(roomAmenity);
                }
                db.SaveChanges();
                RoomAmenitySaved?.Invoke(roomAmenity);
            }
            System.Windows.MessageBox.Show("Lưu chi tiết tiện nghi thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 