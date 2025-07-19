using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using System.Windows.Threading;
using static HotelManagementSystem.ViewModels.AccountManagementViewModel;

namespace HotelManagementSystem.ViewModels
{
    public class StaffManagementViewModel : ViewModelBase
    {
        public ICommand AddStaffCommand { get; }
        public ICommand EditStaffCommand { get; }
        public ICommand DeleteStaffCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        //public event Action? AddStaffRequested;
        //public event Action<StaffDisplay>? EditStaffRequested;
       
        private AccountDisplay _selectedAccount;
        public event Action<Staff?>? RequestAddEditStaff;

        private StaffDisplay _selectedStaff;
        public StaffDisplay SelectedStaff
        {
            get => _selectedStaff;
            set => SetProperty(ref _selectedStaff, value);
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

        public class StaffDisplay
        {
            public int StaffId { get; set; }
            public string FullName { get; set; }
            public string Email { get; set; }
            public string Phone { get; set; }
            public string Position { get; set; }
        }

        private ObservableCollection<StaffDisplay> _staffList;
        public ObservableCollection<StaffDisplay> StaffList
        {
            get => _staffList;
            set => SetProperty(ref _staffList, value);
        }

        public StaffManagementViewModel()
        {
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadStaff();
            };

            AddStaffCommand = new RelayCommand(_ => RequestAddEditStaff?.Invoke(null));
            EditStaffCommand = new RelayCommand(param =>
            {
                var staff = param as StaffDisplay;
                if (staff != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbStaff = db.Staff.FirstOrDefault(s => s.StaffId == staff.StaffId);
                        if (dbStaff != null)
                            RequestAddEditStaff?.Invoke(dbStaff);
                    }
                }
            });
            DeleteStaffCommand = new RelayCommand(param => DeleteStaff(param as StaffDisplay));
            SearchCommand = new RelayCommand(_ => LoadStaff());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadStaff();
            });
            LoadStaff();
        }

        public void LoadStaff()
        {
            using (var db = new HotelManagementDbContext())
            {
                var query = db.Staff.AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(s => 
                        (s.FullName != null && s.FullName.ToLower().Contains(keyword)) ||
                        (s.Email != null && s.Email.ToLower().Contains(keyword)) ||
                        (s.Phone != null && s.Phone.ToLower().Contains(keyword)) ||
                        (s.Position != null && s.Position.ToLower().Contains(keyword))
                    );
                }

                var staffs = query.ToList();
                StaffList = new ObservableCollection<StaffDisplay>(
                    staffs.Select(s => new StaffDisplay
                    {
                        StaffId = s.StaffId,
                        FullName = s.FullName ?? "",
                        Email = s.Email ?? "",
                        Phone = s.Phone ?? "",
                        Position = s.Position ?? ""
                    })
                );
            }
        }

        private void DeleteStaff(StaffDisplay staff)
        {
            if (staff == null) return;
            var result = MessageBox.Show($"Khi xóa nhân viên sẽ xóa cả tài khoản có liên quan. Bạn có chắc chắn muốn xóa nhân viên '{staff.FullName}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbStaff = db.Staff.FirstOrDefault(s => s.StaffId == staff.StaffId);
                if (dbStaff != null)
                {
                    // Xóa các Account liên quan trước
                    var relatedAccounts = db.Accounts.Where(a => a.StaffId == dbStaff.StaffId).ToList();
                    db.Accounts.RemoveRange(relatedAccounts);
                    // Xóa staff
                    db.Staff.Remove(dbStaff);
                    db.SaveChanges();
                }
            }
            LoadStaff();
        }
    }
} 