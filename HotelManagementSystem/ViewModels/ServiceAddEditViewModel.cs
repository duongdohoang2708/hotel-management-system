using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private int _serviceId;
        public int ServiceId
        {
            get => _serviceId;
            set => SetProperty(ref _serviceId, value);
        }

        private string _serviceName;
        public string ServiceName
        {
            get => _serviceName;
            set
            {
                if (SetProperty(ref _serviceName, value))
                {
                    ValidateServiceName();
                }
            }
        }

        private decimal _unitPrice;
        public decimal UnitPrice
        {
            get => _unitPrice;
            set
            {
                if (SetProperty(ref _unitPrice, value))
                {
                    ValidatePrice();
                }
            }
        }

        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set
            {
                if (SetProperty(ref _categoryId, value))
                {
                    ValidateCategoryId();
                }
            }
        }

        private ServiceCategory _selectedCategory;
        public ServiceCategory SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    if (value != null)
                    {
                        CategoryId = value.CategoryId;
                    }
                    ValidateCategoryId();
                }
            }
        }

        private ObservableCollection<ServiceCategory> _serviceCategories;
        public ObservableCollection<ServiceCategory> ServiceCategories
        {
            get => _serviceCategories;
            set => SetProperty(ref _serviceCategories, value);
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa dịch vụ" : "Thêm dịch vụ";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Service>? ServiceSaved;

        private readonly Dictionary<string, List<string>> _errors = new();
        public bool HasErrors => _errors.Count > 0;
        public event EventHandler<DataErrorsChangedEventArgs>? ErrorsChanged;
        public IEnumerable GetErrors(string? propertyName)
        {
            if (string.IsNullOrEmpty(propertyName))
                return null;
            return _errors.ContainsKey(propertyName) ? _errors[propertyName] : null;
        }
        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName))
                _errors[propertyName] = new List<string>();
            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }

        public bool CanSave
        {
            get
            {
                return !string.IsNullOrWhiteSpace(ServiceName)
                    && UnitPrice > 0
                    && CategoryId > 0;
            }
        }

        public ServiceAddEditViewModel(bool isEditMode = false, Service? service = null)
        {
            IsEditMode = isEditMode;
            LoadServiceCategories();
            
            if (isEditMode && service != null)
            {
                ServiceId = service.ServiceId;
                ServiceName = service.ServiceName ?? "";
                UnitPrice = service.UnitPrice ?? 0;
                CategoryId = service.CategoryId;
                SelectedCategory = ServiceCategories.FirstOrDefault(c => c.CategoryId == service.CategoryId);
            }
            
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void LoadServiceCategories()
        {
            using (var db = new HotelManagementDbContext())
            {
                var categories = db.ServiceCategories.ToList();
                ServiceCategories = new ObservableCollection<ServiceCategory>(categories);
            }
        }

        private void Save()
        {
            ValidateServiceName();
            ValidatePrice();
            ValidateCategoryId();
            if (HasErrors)
            {
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var s = db.Services.Find(ServiceId);
                    if (s != null)
                    {
                        s.ServiceName = ServiceName;
                        s.UnitPrice = UnitPrice;
                        s.CategoryId = CategoryId;
                        db.SaveChanges();
                        ServiceSaved?.Invoke(s);
                    }
                }
                else
                {
                    var s = new Service
                    {
                        ServiceName = ServiceName,
                        UnitPrice = UnitPrice,
                        CategoryId = CategoryId
                    };
                    db.Services.Add(s);
                    db.SaveChanges();
                    ServiceSaved?.Invoke(s);
                }
            }
            RequestClose?.Invoke();
        }

        public void ValidateServiceName()
        {
            ClearErrors(nameof(ServiceName));
            if (string.IsNullOrWhiteSpace(ServiceName))
            {
                AddError(nameof(ServiceName), "Tên dịch vụ không được để trống!");
            }
        }

        public void ValidatePrice()
        {
            ClearErrors(nameof(UnitPrice));
            if (UnitPrice <= 0)
            {
                AddError(nameof(UnitPrice), "Giá dịch vụ phải lớn hơn 0!");
            }
        }

        public void ValidateCategoryId()
        {
            ClearErrors(nameof(CategoryId));
            if (CategoryId <= 0)
            {
                AddError(nameof(CategoryId), "Vui lòng chọn loại dịch vụ!");
            }
        }
    }
} 