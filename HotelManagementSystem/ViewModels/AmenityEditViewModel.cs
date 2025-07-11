using System;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Text.RegularExpressions;

namespace HotelManagementSystem.ViewModels
{
    public class AmenityEditViewModel : ViewModelBase
    {
        private string _amenityName = string.Empty;
        public string AmenityName
        {
            get => _amenityName;
            set => SetProperty(ref _amenityName, value);
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Amenity>? AmenitySaved;

        private readonly Amenity? _editingAmenity;
        private readonly bool _isEditMode;

        public AmenityEditViewModel(Amenity? amenity = null)
        {
            if (amenity != null)
            {
                _isEditMode = true;
                _editingAmenity = amenity;
                AmenityName = amenity.AmenityName;
                Description = amenity.Description;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private bool Validate()
        {
            // Validate tên tiện nghi
            if (string.IsNullOrWhiteSpace(AmenityName))
            {
                System.Windows.MessageBox.Show("Tên tiện nghi không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Không cho phép ký tự đặc biệt
            if (!Regex.IsMatch(AmenityName, @"^[\p{L}\d ]+$"))
            {
                System.Windows.MessageBox.Show("Tên tiện nghi không được chứa ký tự đặc biệt!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Kiểm tra trùng tên tiện nghi
            using (var db = new HotelManagementDbContext())
            {
                var existing = db.Amenities.FirstOrDefault(a => a.AmenityName == AmenityName);
                if (existing != null && (!_isEditMode || existing.AmenityId != _editingAmenity?.AmenityId))
                {
                    System.Windows.MessageBox.Show($"Tên tiện nghi '{AmenityName}' đã tồn tại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                Amenity amenity;
                if (_isEditMode && _editingAmenity != null)
                {
                    amenity = db.Amenities.FirstOrDefault(a => a.AmenityId == _editingAmenity.AmenityId) ?? _editingAmenity;
                    amenity.AmenityName = AmenityName;
                    amenity.Description = Description;
                }
                else
                {
                    amenity = new Amenity
                    {
                        AmenityName = AmenityName,
                        Description = Description
                    };
                    db.Amenities.Add(amenity);
                }
                db.SaveChanges();
                AmenitySaved?.Invoke(amenity);
            }
            System.Windows.MessageBox.Show("Lưu tiện nghi thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 