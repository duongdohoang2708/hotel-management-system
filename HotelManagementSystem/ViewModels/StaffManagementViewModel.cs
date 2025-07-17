using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Views.Windows;
using System.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class StaffManagementViewModel : ViewModelBase
    {
        public class StaffDisplay : Staff
        {
     
        }

        private ObservableCollection<StaffDisplay> _staffList;
        public ObservableCollection<StaffDisplay> StaffList
        {
            get => _staffList;
            set => SetProperty(ref _staffList, value);
        }

        private ObservableCollection<StaffDisplay> _filteredStaffList;
        public ObservableCollection<StaffDisplay> FilteredStaffList
        {
            get => _filteredStaffList;
            set => SetProperty(ref _filteredStaffList, value);
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { if (SetProperty(ref _searchKeyword, value)) FilterStaff(); }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddCommand { get; }

        public StaffManagementViewModel()
        {
            LoadStaff();
            EditCommand = new RelayCommand(e => EditStaff(e as StaffDisplay));
            DeleteCommand = new RelayCommand(e => DeleteStaff(e as StaffDisplay));
            AddCommand = new RelayCommand(_ => AddStaff());
        }

        private void LoadStaff()
        {
            using (var db = new HotelManagementDbContext())
            {
                StaffList = new ObservableCollection<StaffDisplay>(
                    db.Staff.Include(s => s.Accounts).ToList().Select(s => new StaffDisplay
                    {
                        StaffId = s.StaffId,
                        FullName = s.FullName,
                        Role = s.Role,
                        Phone = s.Phone,
                        Email = s.Email,
                        
                    })
                );
            }
            FilteredStaffList = new ObservableCollection<StaffDisplay>(StaffList);
        }

        private void FilterStaff()
        {
            var keyword = SearchKeyword?.Trim().ToLower() ?? "";
            var query = StaffList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(s =>
                    (!string.IsNullOrEmpty(s.FullName) && s.FullName.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(s.Phone) && s.Phone.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(s.Role) && s.Role.ToLower().Contains(keyword))
                );
            }

            FilteredStaffList = new ObservableCollection<StaffDisplay>(query);
        }

        private void EditStaff(StaffDisplay? staff)
        {
            if (staff == null) return;
            var editWindow = new StaffEditWindow();
            var vm = new StaffEditViewModel(staff);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.EmployeeSaved += _ => LoadStaff();
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void DeleteStaff(StaffDisplay? staff)
        {
            if (staff == null) return;
            if (AppSession.CurrentAccount != null && AppSession.CurrentAccount.StaffId == staff.StaffId)
            {
                System.Windows.MessageBox.Show("Không thể xóa nhân viên đang đăng nhập!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            var result = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên '{staff.FullName}'?", "Xác nhận xóa", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result != System.Windows.MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var accounts = db.Accounts.Where(a => a.StaffId == staff.StaffId).ToList();
                db.Accounts.RemoveRange(accounts);
                var dbStaff = db.Staff.FirstOrDefault(s => s.StaffId == staff.StaffId);
                if (dbStaff != null)
                {
                    db.Staff.Remove(dbStaff);
                    db.SaveChanges();
                }
            }
            LoadStaff();
        }

        private void AddStaff()
        {
            var addWindow = new StaffEditWindow();
            var vm = new StaffEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.EmployeeSaved += _ => LoadStaff();
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 