using System;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Text.RegularExpressions;

namespace HotelManagementSystem.ViewModels
{
    public class FacilityEditViewModel : ViewModelBase
    {
        private string _facilityName = string.Empty;
        public string FacilityName
        {
            get => _facilityName;
            set => SetProperty(ref _facilityName, value);
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
        public event Action<Facility>? FacilitySaved;

        private readonly Facility? _editingFacility;
        private readonly bool _isEditMode;

        public FacilityEditViewModel(Facility? facility = null)
        {
            if (facility != null)
            {
                _isEditMode = true;
                _editingFacility = facility;
                FacilityName = facility.FacilityName;
                Description = facility.Description;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private bool Validate()
        {
            // Validate tên tiện nghi
            if (string.IsNullOrWhiteSpace(FacilityName))
            {
                System.Windows.MessageBox.Show("Tên tiện nghi không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Không cho phép ký tự đặc biệt
            if (!Regex.IsMatch(FacilityName, @"^[\p{L}\d ]+$"))
            {
                System.Windows.MessageBox.Show("Tên tiện nghi không được chứa ký tự đặc biệt!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Kiểm tra trùng tên tiện nghi
            using (var db = new HotelManagementDbContext())
            {
                var existing = db.Facilities.FirstOrDefault(a => a.FacilityName == FacilityName);
                if (existing != null && (!_isEditMode || existing.FacilityId != _editingFacility?.FacilityId))
                {
                    System.Windows.MessageBox.Show($"Tên tiện nghi '{FacilityName}' đã tồn tại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                Facility facility;
                if (_isEditMode && _editingFacility != null)
                {
                    facility = db.Facilities.FirstOrDefault(a => a.FacilityId == _editingFacility.FacilityId) ?? _editingFacility;
                    facility.FacilityName = FacilityName;
                    facility.Description = Description;
                }
                else
                {
                    facility = new Facility
                    {
                        FacilityName = FacilityName,
                        Description = Description
                    };
                    db.Facilities.Add(facility);
                }
                db.SaveChanges();
                FacilitySaved?.Invoke(facility);
            }
            System.Windows.MessageBox.Show("Lưu tiện nghi thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 