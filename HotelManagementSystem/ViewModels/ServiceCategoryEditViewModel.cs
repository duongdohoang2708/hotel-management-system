using System;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Text.RegularExpressions;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceCategoryEditViewModel : ViewModelBase
    {
        private string _categoryName = string.Empty;
        public string CategoryName
        {
            get => _categoryName;
            set => SetProperty(ref _categoryName, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<ServiceCategory>? CategorySaved;

        private readonly ServiceCategory? _editingCategory;
        private readonly bool _isEditMode;

        public ServiceCategoryEditViewModel(ServiceCategory? category = null)
        {
            if (category != null)
            {
                _isEditMode = true;
                _editingCategory = category;
                CategoryName = category.CategoryName;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private bool Validate()
        {
            // Validate tên loại dịch vụ
            if (string.IsNullOrWhiteSpace(CategoryName))
            {
                System.Windows.MessageBox.Show("Tên loại dịch vụ không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Không cho phép ký tự đặc biệt
            if (!Regex.IsMatch(CategoryName, @"^[\p{L}\d ]+$"))
            {
                System.Windows.MessageBox.Show("Tên loại dịch vụ không được chứa ký tự đặc biệt!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Kiểm tra trùng tên loại dịch vụ
            using (var db = new HotelManagementDbContext())
            {
                var existing = db.ServiceCategories.FirstOrDefault(c => c.CategoryName == CategoryName);
                if (existing != null && (!_isEditMode || existing.CategoryId != _editingCategory?.CategoryId))
                {
                    System.Windows.MessageBox.Show($"Tên loại dịch vụ '{CategoryName}' đã tồn tại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                ServiceCategory category;
                if (_isEditMode && _editingCategory != null)
                {
                    category = db.ServiceCategories.FirstOrDefault(c => c.CategoryId == _editingCategory.CategoryId) ?? _editingCategory;
                    category.CategoryName = CategoryName;
                }
                else
                {
                    category = new ServiceCategory
                    {
                        CategoryName = CategoryName
                    };
                    db.ServiceCategories.Add(category);
                }
                db.SaveChanges();
                CategorySaved?.Invoke(category);
            }
            System.Windows.MessageBox.Show("Lưu loại dịch vụ thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 