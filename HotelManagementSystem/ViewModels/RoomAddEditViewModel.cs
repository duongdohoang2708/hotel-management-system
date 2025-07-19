using System;
using System.Collections.ObjectModel;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Linq;
using Microsoft.EntityFrameworkCore;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace HotelManagementSystem.ViewModels
{
    public class RoomAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private int _roomId;
        public int RoomId
        {
            get => _roomId;
            set => SetProperty(ref _roomId, value);
        }

        private string _roomNumber;
        public string RoomNumber
        {
            get => _roomNumber;
            set
            {
                if (SetProperty(ref _roomNumber, value))
                {
                    ValidateRoomNumber();
                }
            }
        }

        private string _status;
        public string Status
        {
            get => _status;
            set
            {
                if (SetProperty(ref _status, value))
                {
                    ValidateStatus();
                }
            }
        }

        private string _cleanStatus;
        public string CleanStatus
        {
            get => _cleanStatus;
            set
            {
                if (SetProperty(ref _cleanStatus, value))
                {
                    ValidateCleanStatus();
                }
            }
        }

        private int _roomTypeId;
        public int RoomTypeId
        {
            get => _roomTypeId;
            set
            {
                if (SetProperty(ref _roomTypeId, value))
                {
                    ValidateRoomTypeId();
                }
            }
        }

        private RoomType _selectedRoomType;
        public RoomType SelectedRoomType
        {
            get => _selectedRoomType;
            set
            {
                if (SetProperty(ref _selectedRoomType, value))
                {
                    if (value != null)
                    {
                        RoomTypeId = value.RoomTypeId;
                    }
                    ValidateRoomTypeId();
                }
            }
        }

        private ObservableCollection<RoomType> _roomTypes;
        public ObservableCollection<RoomType> RoomTypes
        {
            get => _roomTypes;
            set => SetProperty(ref _roomTypes, value);
        }

        private ObservableCollection<string> _statusOptions;
        public ObservableCollection<string> StatusOptions
        {
            get => _statusOptions;
            set => SetProperty(ref _statusOptions, value);
        }

        private ObservableCollection<string> _cleanStatusOptions;
        public ObservableCollection<string> CleanStatusOptions
        {
            get => _cleanStatusOptions;
            set => SetProperty(ref _cleanStatusOptions, value);
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa phòng" : "Thêm phòng";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Room>? RoomSaved;

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
                return !string.IsNullOrWhiteSpace(RoomNumber)
                    && !string.IsNullOrWhiteSpace(Status)
                    && !string.IsNullOrWhiteSpace(CleanStatus)
                    && RoomTypeId > 0;
            }
        }

        public RoomAddEditViewModel(bool isEditMode = false, Room? room = null)
        {
            IsEditMode = isEditMode;
            LoadRoomTypes();
            LoadStatusOptions();
            
            if (isEditMode && room != null)
            {
                RoomId = room.RoomId;
                RoomNumber = room.RoomNumber ?? "";
                Status = room.Status ?? "";
                CleanStatus = room.CleanStatus ?? "";
                RoomTypeId = room.RoomTypeId;
                SelectedRoomType = RoomTypes.FirstOrDefault(rt => rt.RoomTypeId == room.RoomTypeId);
            }
            
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void LoadRoomTypes()
        {
            using (var db = new HotelManagementDbContext())
            {
                var roomTypes = db.RoomTypes.ToList();
                RoomTypes = new ObservableCollection<RoomType>(roomTypes);
            }
        }

        private void LoadStatusOptions()
        {
            StatusOptions = new ObservableCollection<string>
            {
                "Trống",
                "Đã đặt",
                "Bảo trì",
                "Đang sử dụng"
            };

            CleanStatusOptions = new ObservableCollection<string>
            {
                "Đã dọn",
                "Chưa dọn",
                "Đang dọn"
            };
        }

        public void ValidateRoomNumber()
        {
            ClearErrors(nameof(RoomNumber));
            if (string.IsNullOrWhiteSpace(RoomNumber))
            {
                AddError(nameof(RoomNumber), "Số phòng không được để trống!");
            }
        }
        public void ValidateStatus()
        {
            ClearErrors(nameof(Status));
            if (string.IsNullOrWhiteSpace(Status))
            {
                AddError(nameof(Status), "Trạng thái không được để trống!");
            }
        }
        public void ValidateCleanStatus()
        {
            ClearErrors(nameof(CleanStatus));
            if (string.IsNullOrWhiteSpace(CleanStatus))
            {
                AddError(nameof(CleanStatus), "Tình trạng dọn dẹp không được để trống!");
            }
        }
        public void ValidateRoomTypeId()
        {
            ClearErrors(nameof(RoomTypeId));
            if (RoomTypeId <= 0)
            {
                AddError(nameof(RoomTypeId), "Vui lòng chọn loại phòng!");
            }
        }

        private void Save()
        {
            ValidateRoomNumber();
            ValidateStatus();
            ValidateCleanStatus();
            ValidateRoomTypeId();
            if (HasErrors)
            {
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var r = db.Rooms.Find(RoomId);
                    if (r != null)
                    {
                        r.RoomNumber = RoomNumber;
                        r.Status = Status;
                        r.CleanStatus = CleanStatus;
                        r.RoomTypeId = RoomTypeId;
                        db.SaveChanges();
                        RoomSaved?.Invoke(r);
                    }
                }
                else
                {
                    // Kiểm tra trùng số hiệu phòng
                    bool isDuplicate = db.Rooms.Any(x => x.RoomNumber == RoomNumber);
                    if (isDuplicate)
                    {
                        System.Windows.MessageBox.Show("Số hiệu phòng đã tồn tại. Vui lòng nhập số hiệu khác!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                        return;
                    }
                    var r = new Room
                    {
                        RoomNumber = RoomNumber,
                        Status = Status,
                        CleanStatus = CleanStatus,
                        RoomTypeId = RoomTypeId
                    };
                    db.Rooms.Add(r);
                    db.SaveChanges();
                    RoomSaved?.Invoke(r);
                }
            }
            RequestClose?.Invoke();
        }
    }
} 