using System;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;

namespace HotelManagementSystem.ViewModels
{
    public class GuestAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private int _guestId;
        public int GuestId
        {
            get => _guestId;
            set => SetProperty(ref _guestId, value);
        }

        private string? _fullName;
        public string? FullName
        {
            get => _fullName;
            set
            {
                if (SetProperty(ref _fullName, value))
                {
                    ValidateFullName();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private string? _address;
        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private string? _phone;
        public string? Phone
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

        private string? _idCardNo;
        public string? IdCardNo
        {
            get => _idCardNo;
            set
            {
                if (SetProperty(ref _idCardNo, value))
                {
                    ValidateIdCardNo();
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa thông tin khách hàng" : "Thêm khách hàng mới";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Guest>? GuestSaved;

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
                return !HasErrors
                    && !string.IsNullOrWhiteSpace(FullName)
                    && !string.IsNullOrWhiteSpace(Phone)
                    && !string.IsNullOrWhiteSpace(IdCardNo);
            }
        }

        public GuestAddEditViewModel(bool isEditMode = false, Guest? guest = null)
        {
            IsEditMode = isEditMode;
            if (isEditMode && guest != null)
            {
                GuestId = guest.GuestId;
                FullName = guest.FullName;
                Address = guest.Address;
                Phone = guest.Phone;
                IdCardNo = guest.IdCardNo;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void Save()
        {
            // Gọi validate cho tất cả các trường bắt buộc trước khi lưu
            ValidateFullName();
            ValidatePhone();
            ValidateIdCardNo();
            if (HasErrors)
            {
                System.Windows.MessageBox.Show("Vui lòng nhập đầy đủ và đúng thông tin cho các trường bắt buộc!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var g = db.Guests.Find(GuestId);
                    if (g != null)
                    {
                        g.FullName = FullName;
                        g.Address = Address;
                        g.Phone = Phone;
                        g.IdCardNo = IdCardNo;
                        db.SaveChanges();
                        GuestSaved?.Invoke(g);
                    }
                }
                else
                {
                    var g = new Guest
                    {
                        FullName = FullName,
                        Address = Address,
                        Phone = Phone,
                        IdCardNo = IdCardNo
                    };
                    db.Guests.Add(g);
                    db.SaveChanges();
                    GuestSaved?.Invoke(g);
                }
            }
            RequestClose?.Invoke();
        }

        public void ValidateFullName()
        {
            ClearErrors(nameof(FullName));
            if (string.IsNullOrWhiteSpace(FullName))
            {
                AddError(nameof(FullName), "Tên khách không được để trống!");
            }
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
                AddError(nameof(Phone), "Số điện thoại phải gồm 10 chữ số và bắt đầu bằng số 0!");
            }
        }
        public void ValidateIdCardNo()
        {
            ClearErrors(nameof(IdCardNo));
            if (string.IsNullOrWhiteSpace(IdCardNo))
            {
                AddError(nameof(IdCardNo), "CMND/CCCD không được để trống!");
            }
            else if (IdCardNo.Length != 12 || !IdCardNo.All(char.IsDigit))
            {
                AddError(nameof(IdCardNo), "CMND/CCCD phải gồm đúng 12 chữ số!");
            }
        }
    }
} 