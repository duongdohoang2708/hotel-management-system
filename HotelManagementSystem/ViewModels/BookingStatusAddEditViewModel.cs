using System;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace HotelManagementSystem.ViewModels
{
    public class BookingStatusAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private int _statusId;
        public int StatusId
        {
            get => _statusId;
            set => SetProperty(ref _statusId, value);
        }

        private string? _statusName;
        public string? StatusName
        {
            get => _statusName;
            set
            {
                if (SetProperty(ref _statusName, value))
                {
                    ValidateStatusName();
                }
            }
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa trạng thái đặt phòng" : "Thêm trạng thái đặt phòng";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<BookingStatus>? BookingStatusSaved;

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
                return !string.IsNullOrWhiteSpace(StatusName);
            }
        }

        public BookingStatusAddEditViewModel(bool isEditMode = false, BookingStatus? bookingStatus = null)
        {
            IsEditMode = isEditMode;
            if (isEditMode && bookingStatus != null)
            {
                StatusId = bookingStatus.StatusId;
                StatusName = bookingStatus.StatusName;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void Save()
        {
            ValidateStatusName();
            if (HasErrors)
            {
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var bs = db.BookingStatuses.Find(StatusId);
                    if (bs != null)
                    {
                        bs.StatusName = StatusName;
                        db.SaveChanges();
                        BookingStatusSaved?.Invoke(bs);
                    }
                }
                else
                {
                    var bs = new BookingStatus
                    {
                        StatusName = StatusName
                    };
                    db.BookingStatuses.Add(bs);
                    db.SaveChanges();
                    BookingStatusSaved?.Invoke(bs);
                }
            }
            RequestClose?.Invoke();
        }

        public void ValidateStatusName()
        {
            ClearErrors(nameof(StatusName));
            if (string.IsNullOrWhiteSpace(StatusName))
            {
                AddError(nameof(StatusName), "Tên trạng thái không được để trống!");
            }
        }
    }
} 