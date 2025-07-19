using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceCategoryManagementViewModel : ViewModelBase
    {
        public ICommand AddServiceCategoryCommand { get; }
        public ICommand EditServiceCategoryCommand { get; }
        public ICommand DeleteServiceCategoryCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public event Action<ServiceCategory?>? RequestAddEditServiceCategory;

        private ServiceCategoryDisplay _selectedServiceCategory;
        public ServiceCategoryDisplay SelectedServiceCategory
        {
            get => _selectedServiceCategory;
            set => SetProperty(ref _selectedServiceCategory, value);
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

        public class ServiceCategoryDisplay
        {
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
        }

        private ObservableCollection<ServiceCategoryDisplay> _serviceCategoryList;
        public ObservableCollection<ServiceCategoryDisplay> ServiceCategoryList
        {
            get => _serviceCategoryList;
            set => SetProperty(ref _serviceCategoryList, value);
        }

        public ServiceCategoryManagementViewModel()
        {
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadServiceCategories();
            };

            AddServiceCategoryCommand = new RelayCommand(_ => RequestAddEditServiceCategory?.Invoke(null));
            EditServiceCategoryCommand = new RelayCommand(param =>
            {
                var serviceCategory = param as ServiceCategoryDisplay;
                if (serviceCategory != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbServiceCategory = db.ServiceCategories.FirstOrDefault(sc => sc.CategoryId == serviceCategory.CategoryId);
                        if (dbServiceCategory != null)
                            RequestAddEditServiceCategory?.Invoke(dbServiceCategory);
                    }
                }
            });
            DeleteServiceCategoryCommand = new RelayCommand(param => DeleteServiceCategory(param as ServiceCategoryDisplay));
            SearchCommand = new RelayCommand(_ => LoadServiceCategories());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadServiceCategories();
            });
            LoadServiceCategories();
        }

        public void LoadServiceCategories()
        {
            using (var db = new HotelManagementDbContext())
            {
                var query = db.ServiceCategories.AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(sc => 
                        (sc.CategoryName != null && sc.CategoryName.ToLower().Contains(keyword))
                    );
                }

                var serviceCategories = query.ToList();
                ServiceCategoryList = new ObservableCollection<ServiceCategoryDisplay>(
                    serviceCategories.Select(sc => new ServiceCategoryDisplay
                    {
                        CategoryId = sc.CategoryId,
                        CategoryName = sc.CategoryName ?? "",
                    })
                );
            }
        }

        private void DeleteServiceCategory(ServiceCategoryDisplay serviceCategory)
        {
            if (serviceCategory == null) return;
            using (var db = new HotelManagementDbContext())
            {
                // Kiểm tra ràng buộc với Service
                bool isInUse = db.Services.Any(s => s.CategoryId == serviceCategory.CategoryId);
                if (isInUse)
                {
                    MessageBox.Show($"Không thể xóa danh mục dịch vụ '{serviceCategory.CategoryName}' vì đã có dịch vụ trong danh mục này!", "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa danh mục dịch vụ '{serviceCategory.CategoryName}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbServiceCategory = db.ServiceCategories.FirstOrDefault(sc => sc.CategoryId == serviceCategory.CategoryId);
                if (dbServiceCategory != null)
                {
                    db.ServiceCategories.Remove(dbServiceCategory);
                    db.SaveChanges();
                }
            }
            LoadServiceCategories();
        }
    }
} 