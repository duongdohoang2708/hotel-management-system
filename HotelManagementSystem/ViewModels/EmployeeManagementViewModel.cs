using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Views.Windows;
using System.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class EmployeeManagementViewModel : ViewModelBase
    {
        public class EmployeeDisplay : Employee
        {
            public string GenderDisplay => Gender == "M" ? "Nam" : (Gender == "F" ? "Nữ" : Gender);
            public string DateOfBirthDisplay => DateOfBirth.HasValue ? DateOfBirth.Value.ToString("dd/MM/yyyy") : "";
        }

        private ObservableCollection<EmployeeDisplay> _employeeList;
        public ObservableCollection<EmployeeDisplay> EmployeeList
        {
            get => _employeeList;
            set => SetProperty(ref _employeeList, value);
        }

        private ObservableCollection<EmployeeDisplay> _filteredEmployeeList;
        public ObservableCollection<EmployeeDisplay> FilteredEmployeeList
        {
            get => _filteredEmployeeList;
            set => SetProperty(ref _filteredEmployeeList, value);
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { if (SetProperty(ref _searchKeyword, value)) FilterEmployees(); }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddCommand { get; }

        public EmployeeManagementViewModel()
        {
            LoadEmployees();
            EditCommand = new RelayCommand(e => EditEmployee(e as EmployeeDisplay));
            DeleteCommand = new RelayCommand(e => DeleteEmployee(e as EmployeeDisplay));
            AddCommand = new RelayCommand(_ => AddEmployee());
        }

        private void LoadEmployees()
        {
            using (var db = new HotelManagementDbContext())
            {
                EmployeeList = new ObservableCollection<EmployeeDisplay>(
                    db.Employees.Include(e => e.Accounts).ToList().Select(e => new EmployeeDisplay
                    {
                        EmployeeId = e.EmployeeId,
                        FullName = e.FullName,
                        Position = e.Position,
                        Phone = e.Phone,
                        Email = e.Email,
                        DateOfBirth = e.DateOfBirth,
                        Gender = e.Gender,
                        Address = e.Address,
                        CitizenId = e.CitizenId,
                        Salary = e.Salary
                    })
                );
            }
            FilteredEmployeeList = new ObservableCollection<EmployeeDisplay>(EmployeeList);
        }

        private void FilterEmployees()
        {
            var keyword = SearchKeyword?.Trim().ToLower() ?? "";
            var query = EmployeeList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(e =>
                    (!string.IsNullOrEmpty(e.FullName) && e.FullName.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(e.Phone) && e.Phone.ToLower().Contains(keyword)) ||
                    (!string.IsNullOrEmpty(e.Position) && e.Position.ToLower().Contains(keyword))
                );
            }

            FilteredEmployeeList = new ObservableCollection<EmployeeDisplay>(query);
        }

        private void EditEmployee(EmployeeDisplay? employee)
        {
            if (employee == null) return;
            var editWindow = new EmployeeEditWindow();
            var vm = new EmployeeEditViewModel(employee);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.EmployeeSaved += _ => LoadEmployees();
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void DeleteEmployee(EmployeeDisplay? employee)
        {
            if (employee == null) return;
            if (AppSession.CurrentAccount != null && AppSession.CurrentAccount.EmployeeId == employee.EmployeeId)
            {
                System.Windows.MessageBox.Show("Không thể xóa nhân viên đang đăng nhập!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            var result = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn xóa nhân viên '{employee.FullName}'?", "Xác nhận xóa", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result != System.Windows.MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var accounts = db.Accounts.Where(a => a.EmployeeId == employee.EmployeeId).ToList();
                db.Accounts.RemoveRange(accounts);
                var dbEmp = db.Employees.FirstOrDefault(e => e.EmployeeId == employee.EmployeeId);
                if (dbEmp != null)
                {
                    db.Employees.Remove(dbEmp);
                    db.SaveChanges();
                }
            }
            LoadEmployees();
        }

        private void AddEmployee()
        {
            var addWindow = new EmployeeEditWindow();
            var vm = new EmployeeEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.EmployeeSaved += _ => LoadEmployees();
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 