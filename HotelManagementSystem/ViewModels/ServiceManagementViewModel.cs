using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceManagementViewModel : ViewModelBase
    {
        public ICommand AddServiceCommand { get; }
        public ICommand EditServiceCommand { get; }
        public ICommand DeleteServiceCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public event Action<Service?>? RequestAddEditService;

        private ServiceDisplay _selectedService;
        public ServiceDisplay SelectedService
        {
            get => _selectedService;
            set => SetProperty(ref _selectedService, value);
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

        public class ServiceDisplay
        {
            public int ServiceId { get; set; }
            public string ServiceName { get; set; }
            public decimal UnitPrice { get; set; }
            public int CategoryId { get; set; }
            public string CategoryName { get; set; }
        }

        private ObservableCollection<ServiceDisplay> _serviceList;
        public ObservableCollection<ServiceDisplay> ServiceList
        {
            get => _serviceList;
            set => SetProperty(ref _serviceList, value);
        }

        public ServiceManagementViewModel()
        {
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadServices();
            };

            AddServiceCommand = new RelayCommand(_ => RequestAddEditService?.Invoke(null));
            EditServiceCommand = new RelayCommand(param =>
            {
                var service = param as ServiceDisplay;
                if (service != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbService = db.Services.FirstOrDefault(s => s.ServiceId == service.ServiceId);
                        if (dbService != null)
                            RequestAddEditService?.Invoke(dbService);
                    }
                }
            });
            DeleteServiceCommand = new RelayCommand(param => DeleteService(param as ServiceDisplay));
            SearchCommand = new RelayCommand(_ => LoadServices());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadServices();
            });
            LoadServices();
        }

        public void LoadServices()
        {
            using (var db = new HotelManagementDbContext())
            {
                var query = db.Services.Include(s => s.Category).AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(s => 
                        (s.ServiceName != null && s.ServiceName.ToLower().Contains(keyword)) ||
                        (s.Category != null && s.Category.CategoryName != null && s.Category.CategoryName.ToLower().Contains(keyword)) ||
                        s.UnitPrice.ToString().Contains(keyword)
                    );
                }

                var services = query.ToList();
                ServiceList = new ObservableCollection<ServiceDisplay>(
                    services.Select(s => new ServiceDisplay
                    {
                        ServiceId = s.ServiceId,
                        ServiceName = s.ServiceName ?? "",
                        UnitPrice = s.UnitPrice ?? 0,
                        CategoryId = s.CategoryId,
                        CategoryName = s.Category?.CategoryName ?? ""
                    })
                );
            }
        }

        private void DeleteService(ServiceDisplay service)
        {
            if (service == null) return;
            using (var db = new HotelManagementDbContext())
            {
                // Kiểm tra ràng buộc với RoomServiceUsage
                bool isInUse = db.RoomServiceUsages.Any(rsu => rsu.ServiceId == service.ServiceId);
                if (isInUse)
                {
                    MessageBox.Show($"Không thể xóa dịch vụ '{service.ServiceName}' vì đã có lịch sử sử dụng!", "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa dịch vụ '{service.ServiceName}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbService = db.Services.FirstOrDefault(s => s.ServiceId == service.ServiceId);
                if (dbService != null)
                {
                    db.Services.Remove(dbService);
                    db.SaveChanges();
                }
            }
            LoadServices();
        }
    }
} 