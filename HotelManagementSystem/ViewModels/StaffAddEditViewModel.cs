using System;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Linq;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace HotelManagementSystem.ViewModels
{
    public class StaffAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private int _staffId;
        public int StaffId
        {
            get => _staffId;
            set => SetProperty(ref _staffId, value);
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    ValidateStaffName();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private string _email;
        public string Email
        {
            get => _email;
            set
            {
                if (SetProperty(ref _email, value))
                {
                    ValidateEmail();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set
            {
                if (SetProperty(ref _phone, value))
                {
                    ValidatePhone();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private string _position;
        public string Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa nhân viên" : "Thêm nhân viên";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Staff>? StaffSaved;

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
                return !string.IsNullOrWhiteSpace(FullName)
                    && IsValidEmail(Email)
                    && IsValidPhone(Phone);
            }
        }

        public string FullNameError => _errors.ContainsKey(nameof(FullName)) ? _errors[nameof(FullName)].FirstOrDefault() : null;
        public string EmailError => _errors.ContainsKey(nameof(Email)) ? _errors[nameof(Email)].FirstOrDefault() : null;
        public string PhoneError => _errors.ContainsKey(nameof(Phone)) ? _errors[nameof(Phone)].FirstOrDefault() : null;

        public StaffAddEditViewModel(bool isEditMode = false, Staff? staff = null)
        {
            IsEditMode = isEditMode;
            if (isEditMode && staff != null)
            {
                StaffId = staff.StaffId;
                FullName = staff.FullName;
                Email = staff.Email;
                Phone = staff.Phone;
                Position = staff.Position;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void Save()
        {
            ValidateStaffName();
            ValidateEmail();
            ValidatePhone();
            if (HasErrors)
            {
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var s = db.Staff.Find(StaffId);
                    if (s != null)
                    {
                        s.FullName = FullName;
                        s.Email = Email;
                        s.Phone = Phone;
                        s.Position = Position;
                        db.SaveChanges();
                        StaffSaved?.Invoke(s);
                    }
                }
                else
                {
                    var s = new Staff
                    {
                        FullName = FullName,
                        Email = Email,
                        Phone = Phone,
                        Position = Position
                    };
                    db.Staff.Add(s);
                    db.SaveChanges();
                    StaffSaved?.Invoke(s);
                }
            }
            RequestClose?.Invoke();
        }

        private bool IsValidEmail(string email)
        {
            if (string.IsNullOrWhiteSpace(email)) return false;
            try
            {
                var addr = new System.Net.Mail.MailAddress(email);
                return addr.Address == email;
            }
            catch
            {
                return false;
            }
        }

        private bool IsValidPhone(string phone)
        {
            return !string.IsNullOrWhiteSpace(phone)
                && phone.Length == 10
                && phone.StartsWith("0")
                && phone.All(char.IsDigit);
        }

        public void ValidateStaffName()
        {
            ClearErrors(nameof(FullName));
            if (string.IsNullOrWhiteSpace(FullName))
            {
                AddError(nameof(FullName), "Tên nhân viên không được để trống!");
            }
            OnPropertyChanged(nameof(FullNameError));
        }
        public void ValidateEmail()
        {
            ClearErrors(nameof(Email));
            if (string.IsNullOrWhiteSpace(Email))
            {
                AddError(nameof(Email), "Email không được để trống!");
            }
            else if (!IsValidEmail(Email))
            {
                AddError(nameof(Email), "Email không hợp lệ!");
            }
            OnPropertyChanged(nameof(EmailError));
        }
        public void ValidatePhone()
        {
            ClearErrors(nameof(Phone));
            if (string.IsNullOrWhiteSpace(Phone))
            {
                AddError(nameof(Phone), "Số điện thoại không được để trống!");
            }
            else if (Phone.Length != 10 || !Phone.All(char.IsDigit) || !Phone.StartsWith("0"))
            {
                AddError(nameof(Phone), "Số điện thoại không hợp lệ!");
            }
            OnPropertyChanged(nameof(PhoneError));
        }
    }
} 