using System;
using System.Windows.Input;
using HotelManagementSystem.Models;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;

namespace HotelManagementSystem.ViewModels
{
    public class RoomTypeAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private int _roomTypeId;
        public int RoomTypeId
        {
            get => _roomTypeId;
            set => SetProperty(ref _roomTypeId, value);
        }

        private string? _typeName;
        public string? TypeName
        {
            get => _typeName;
            set
            {
                if (SetProperty(ref _typeName, value))
                {
                    ValidateTypeName();
                }
            }
        }

        private string? _description;
        public string? Description
        {
            get => _description;
            set
            {
                if (SetProperty(ref _description, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private decimal? _basePrice;
        public decimal? BasePrice
        {
            get => _basePrice;
            set
            {
                if (SetProperty(ref _basePrice, value))
                {
                    ValidatePrice();
                }
            }
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa loại phòng" : "Thêm loại phòng";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<RoomType>? RoomTypeSaved;

        public bool CanSave
        {
            get
            {
                return !string.IsNullOrWhiteSpace(TypeName)
                    && BasePrice.HasValue
                    && BasePrice.Value >= 0;
            }
        }

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

        public RoomTypeAddEditViewModel(bool isEditMode = false, RoomType? roomType = null)
        {
            IsEditMode = isEditMode;
            if (isEditMode && roomType != null)
            {
                RoomTypeId = roomType.RoomTypeId;
                TypeName = roomType.TypeName;
                Description = roomType.Description;
                BasePrice = roomType.BasePrice;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void Save()
        {
            ValidateTypeName();
            ValidatePrice();
            if (HasErrors)
            {
                return;
            }
            if (string.IsNullOrWhiteSpace(TypeName))
            {
                System.Windows.MessageBox.Show("Tên loại phòng không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            if (!BasePrice.HasValue || BasePrice.Value < 0)
            {
                System.Windows.MessageBox.Show("Giá phòng phải là số không âm!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var rt = db.RoomTypes.Find(RoomTypeId);
                    if (rt != null)
                    {
                        rt.TypeName = TypeName;
                        rt.Description = Description;
                        rt.BasePrice = BasePrice;
                        db.SaveChanges();
                        RoomTypeSaved?.Invoke(rt);
                    }
                }
                else
                {
                    var rt = new RoomType
                    {
                        TypeName = TypeName,
                        Description = Description,
                        BasePrice = BasePrice
                    };
                    db.RoomTypes.Add(rt);
                    db.SaveChanges();
                    RoomTypeSaved?.Invoke(rt);
                }
            }
            RequestClose?.Invoke();
        }

        public void ValidateTypeName()
        {
            ClearErrors(nameof(TypeName));
            if (string.IsNullOrWhiteSpace(TypeName))
            {
                AddError(nameof(TypeName), "Tên loại phòng không được để trống!");
            }
        }

        public void ValidatePrice()
        {
            ClearErrors(nameof(BasePrice));
            if (!BasePrice.HasValue || BasePrice.Value < 0)
            {
                AddError(nameof(BasePrice), "Giá phòng phải là số không âm!");
            }
        }
    }
} 