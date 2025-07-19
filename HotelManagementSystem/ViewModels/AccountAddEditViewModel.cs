using HotelManagementSystem.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections;
using System.ComponentModel;

namespace HotelManagementSystem.ViewModels
{
    public class AccountAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private string _username;
        public string Username
        {
            get => _username;
            set
            {
                if (SetProperty(ref _username, value))
                {
                    ValidateUsername();
                }
            }
        }

        private string _password;
        public string Password
        {
            get => _password;
            set
            {
                if (SetProperty(ref _password, value))
                {
                    ValidatePassword();
                }
            }
        }

        private string _role;
        public string Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        private int _staffId;
        public int StaffId
        {
            get => _staffId;
            set => SetProperty(ref _staffId, value);
        }

        public ObservableCollection<Staff> StaffList { get; set; }
        public ObservableCollection<string> Roles { get; set; } = new ObservableCollection<string> { "Admin", "Manager", "Receptionist", "Cashier" };

        public bool IsEditMode { get; set; }
        public int AccountID { get; set; }

        public string WindowTitle => IsEditMode ? "Sửa tài khoản" : "Thêm tài khoản";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Account>? AccountSaved;

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

        public AccountAddEditViewModel(bool isEditMode = false, Account? account = null)
        {
            IsEditMode = isEditMode;
            StaffList = new ObservableCollection<Staff>();
            LoadStaffList();
            if (isEditMode && account != null)
            {
                AccountID = account.AccountId;
                Username = account.Username;
                Password = account.Password;
                Role = account.Role;
                StaffId = account.StaffId;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void LoadStaffList()
        {
            using (var db = new HotelManagementDbContext())
            {
                var staffs = db.Staff.ToList();
                StaffList.Clear();
                foreach (var s in staffs)
                    StaffList.Add(s);
            }
        }

        private void Save()
        {
            ValidateUsername();
            ValidatePassword();
            if (HasErrors)
            {
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var acc = db.Accounts.FirstOrDefault(a => a.AccountId == AccountID);
                    if (acc != null)
                    {
                        acc.Username = Username;
                        acc.Password = Password;
                        acc.Role = Role;
                        db.SaveChanges();
                        AccountSaved?.Invoke(acc);
                    }
                }
                else
                {
                    var acc = new Account
                    {
                        Username = Username,
                        Password = Password,
                        Role = Role,
                        StaffId = StaffId
                    };
                    db.Accounts.Add(acc);
                    db.SaveChanges();
                    AccountSaved?.Invoke(acc);
                }
            }
            RequestClose?.Invoke();
        }

        public void ValidateUsername()
        {
            ClearErrors(nameof(Username));
            if (string.IsNullOrWhiteSpace(Username))
            {
                AddError(nameof(Username), "Tên đăng nhập không được để trống!");
            }
        }
        public void ValidatePassword()
        {
            ClearErrors(nameof(Password));
            if (string.IsNullOrWhiteSpace(Password))
            {
                AddError(nameof(Password), "Mật khẩu không được để trống!");
            }
        }
    }
}
