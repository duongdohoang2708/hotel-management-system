using System;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceCategoryAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private int _categoryId;
        public int CategoryId
        {
            get => _categoryId;
            set => SetProperty(ref _categoryId, value);
        }

        private string? _categoryName;
        public string? CategoryName
        {
            get => _categoryName;
            set
            {
                if (SetProperty(ref _categoryName, value))
                {
                    ValidateCategoryName();
                }
            }
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa danh mục dịch vụ" : "Thêm danh mục dịch vụ";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<ServiceCategory>? ServiceCategorySaved;

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
                return !string.IsNullOrWhiteSpace(CategoryName);
            }
        }

        public ServiceCategoryAddEditViewModel(bool isEditMode = false, ServiceCategory? serviceCategory = null)
        {
            IsEditMode = isEditMode;
            if (isEditMode && serviceCategory != null)
            {
                CategoryId = serviceCategory.CategoryId;
                CategoryName = serviceCategory.CategoryName;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void Save()
        {
            ValidateCategoryName();
            if (HasErrors)
            {
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var sc = db.ServiceCategories.Find(CategoryId);
                    if (sc != null)
                    {
                        sc.CategoryName = CategoryName;
                        db.SaveChanges();
                        ServiceCategorySaved?.Invoke(sc);
                    }
                }
                else
                {
                    var sc = new ServiceCategory
                    {
                        CategoryName = CategoryName
                    };
                    db.ServiceCategories.Add(sc);
                    db.SaveChanges();
                    ServiceCategorySaved?.Invoke(sc);
                }
            }
            RequestClose?.Invoke();
        }

        public void ValidateCategoryName()
        {
            ClearErrors(nameof(CategoryName));
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                AddError(nameof(CategoryName), "Tên loại dịch vụ không được để trống!");
            }
        }
    }
} 