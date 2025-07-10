using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Text.RegularExpressions;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceEditViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceCategory> CategoryList { get; }

        private string _serviceName = string.Empty;
        public string ServiceName
        {
            get => _serviceName;
            set => SetProperty(ref _serviceName, value);
        }

        private ServiceCategory? _selectedCategory;
        public ServiceCategory? SelectedCategory
        {
            get => _selectedCategory;
            set => SetProperty(ref _selectedCategory, value);
        }

        private decimal? _unitPrice;
        public decimal? UnitPrice
        {
            get => _unitPrice;
            set => SetProperty(ref _unitPrice, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Service>? ServiceSaved;

        private readonly Service? _editingService;
        private readonly bool _isEditMode;

        public ServiceEditViewModel(Service? service = null)
        {
            using (var db = new HotelManagementDbContext())
            {
                CategoryList = new ObservableCollection<ServiceCategory>(db.ServiceCategories.ToList());
            }
            if (service != null)
            {
                _isEditMode = true;
                _editingService = service;
                ServiceName = service.ServiceName;
                SelectedCategory = CategoryList.FirstOrDefault(c => c.CategoryId == service.CategoryId);
                UnitPrice = service.UnitPrice;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private bool Validate()
        {
            // Validate tên dịch vụ
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                System.Windows.MessageBox.Show("Tên dịch vụ không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Không cho phép ký tự đặc biệt trong tên dịch vụ
            if (!Regex.IsMatch(ServiceName, @"^[\p{L}\d ]+$"))
            {
                System.Windows.MessageBox.Show("Tên dịch vụ không được chứa ký tự đặc biệt!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Kiểm tra trùng tên dịch vụ
            using (var db = new HotelManagementDbContext())
            {
                var existing = db.Services.FirstOrDefault(s => s.ServiceName == ServiceName);
                if (existing != null && (!_isEditMode || existing.ServiceId != _editingService?.ServiceId))
                {
                    System.Windows.MessageBox.Show($"Tên dịch vụ '{ServiceName}' đã tồn tại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return false;
                }
            }
            // Validate loại dịch vụ
            if (SelectedCategory == null)
            {
                System.Windows.MessageBox.Show("Vui lòng chọn loại dịch vụ!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Validate đơn giá
            if (UnitPrice == null)
            {
                System.Windows.MessageBox.Show("Vui lòng nhập đơn giá!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Đơn giá chỉ được nhập số và lớn hơn 0
            if (!decimal.TryParse(UnitPrice.ToString(), out var price) || price <= 0)
            {
                System.Windows.MessageBox.Show("Đơn giá phải là số và lớn hơn 0!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (UnitPrice <= 0)
            {
                System.Windows.MessageBox.Show("Đơn giá phải lớn hơn 0!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void Save()
        {
            if (!Validate()) return;
            using (var db = new HotelManagementDbContext())
            {
                Service service;
                if (_isEditMode && _editingService != null)
                {
                    service = db.Services.FirstOrDefault(s => s.ServiceId == _editingService.ServiceId) ?? _editingService;
                    service.ServiceName = ServiceName;
                    service.CategoryId = SelectedCategory?.CategoryId ?? service.CategoryId;
                    service.UnitPrice = UnitPrice ?? service.UnitPrice;
                }
                else
                {
                    service = new Service
                    {
                        ServiceName = ServiceName,
                        CategoryId = SelectedCategory?.CategoryId ?? 0,
                        UnitPrice = UnitPrice ?? 0
                    };
                    db.Services.Add(service);
                }
                db.SaveChanges();
                ServiceSaved?.Invoke(service);
            }
            System.Windows.MessageBox.Show("Lưu dịch vụ thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 