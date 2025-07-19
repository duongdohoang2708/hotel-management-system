using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;

namespace HotelManagementSystem.ViewModels
{
    public class FacilityManagementViewModel : ViewModelBase
    {
        public ICommand AddFacilityCommand { get; }
        public ICommand EditFacilityCommand { get; }
        public ICommand DeleteFacilityCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        public event Action<Facility?>? RequestAddEditFacility;

        private FacilityDisplay _selectedFacility;
        public FacilityDisplay SelectedFacility
        {
            get => _selectedFacility;
            set => SetProperty(ref _selectedFacility, value);
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

        public class FacilityDisplay
        {
            public int FacilityId { get; set; }
            public string FacilityName { get; set; }
            public string Description { get; set; }
        }

        private ObservableCollection<FacilityDisplay> _facilityList;
        public ObservableCollection<FacilityDisplay> FacilityList
        {
            get => _facilityList;
            set => SetProperty(ref _facilityList, value);
        }

        public FacilityManagementViewModel()
        {
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadFacilities();
            };

            AddFacilityCommand = new RelayCommand(_ => RequestAddEditFacility?.Invoke(null));
            EditFacilityCommand = new RelayCommand(param =>
            {
                var facility = param as FacilityDisplay;
                if (facility != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbFacility = db.Facilities.FirstOrDefault(f => f.FacilityId == facility.FacilityId);
                        if (dbFacility != null)
                            RequestAddEditFacility?.Invoke(dbFacility);
                    }
                }
            });
            DeleteFacilityCommand = new RelayCommand(param => DeleteFacility(param as FacilityDisplay));
            SearchCommand = new RelayCommand(_ => LoadFacilities());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadFacilities();
            });
            LoadFacilities();
        }

        public void LoadFacilities()
        {
            using (var db = new HotelManagementDbContext())
            {
                var query = db.Facilities.AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(f => 
                        (f.FacilityName != null && f.FacilityName.ToLower().Contains(keyword)) ||
                        (f.Description != null && f.Description.ToLower().Contains(keyword))
                    );
                }

                var facilities = query.ToList();
                FacilityList = new ObservableCollection<FacilityDisplay>(
                    facilities.Select(f => new FacilityDisplay
                    {
                        FacilityId = f.FacilityId,
                        FacilityName = f.FacilityName ?? "",
                        Description = f.Description ?? ""
                    })
                );
            }
        }

        private void DeleteFacility(FacilityDisplay facility)
        {
            if (facility == null) return;
            using (var db = new HotelManagementDbContext())
            {
                // Kiểm tra ràng buộc với RoomFacility
                bool isInUse = db.RoomFacilities.Any(rf => rf.FacilityId == facility.FacilityId);
                if (isInUse)
                {
                    MessageBox.Show($"Không thể xóa thiết bị '{facility.FacilityName}' vì đang được có trong dữ liệu thiết bị theo phòng!", "Không thể xóa", MessageBoxButton.OK, MessageBoxImage.Error);
                    return;
                }
            }
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa thiết bị '{facility.FacilityName}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbFacility = db.Facilities.FirstOrDefault(f => f.FacilityId == facility.FacilityId);
                if (dbFacility != null)
                {
                    db.Facilities.Remove(dbFacility);
                    db.SaveChanges();
                }
            }
            LoadFacilities();
        }
    }
} 