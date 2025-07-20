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
    public class RoomFacilityAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        private int _roomFacilityId;
        public int RoomFacilityId
        {
            get => _roomFacilityId;
            set => SetProperty(ref _roomFacilityId, value);
        }

        private int _roomId;
        public int RoomId
        {
            get => _roomId;
            set => SetProperty(ref _roomId, value);
        }

        private int _facilityId;
        public int FacilityId
        {
            get => _facilityId;
            set
            {
                if (SetProperty(ref _facilityId, value))
                {
                    OnPropertyChanged(nameof(CanSave));
                }
            }
        }

        private int? _quantity;
        public int? Quantity
        {
            get => _quantity;
            set
            {
                if (SetProperty(ref _quantity, value))
                {
                    ValidateQuantity();
                }
            }
        }

        private Facility _selectedFacility;
        public Facility SelectedFacility
        {
            get => _selectedFacility;
            set
            {
                if (SetProperty(ref _selectedFacility, value))
                {
                    ValidateSelectedFacility();
                    FacilityId= value?.FacilityId ?? 0;
                }
            }
        }

        private ObservableCollection<Facility> _facilities;
        public ObservableCollection<Facility> Facilities
        {
            get => _facilities;
            set => SetProperty(ref _facilities, value);
        }

        private string _roomInfo;
        public string RoomInfo
        {
            get => _roomInfo;
            set => SetProperty(ref _roomInfo, value);
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa thiết bị phòng" : "Thêm thiết bị phòng";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<RoomFacility>? RoomFacilitySaved;

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
                return RoomId > 0 && FacilityId > 0 && Quantity.HasValue && Quantity.Value > 0;
            }
        }

        public RoomFacilityAddEditViewModel(bool isEditMode = false, RoomFacility? roomFacility = null, RoomDisplay? roomDisplay = null)
        {
            IsEditMode = isEditMode;
            LoadFacilities();
            
            if (isEditMode && roomFacility != null)
            {
                RoomFacilityId = roomFacility.RoomFacilityId;
                RoomId = roomFacility.RoomId;
                FacilityId = roomFacility.FacilityId;
                Quantity = roomFacility.Quantity;
                SelectedFacility = Facilities.FirstOrDefault(f => f.FacilityId == roomFacility.FacilityId);
                LoadRoomInfo(roomFacility.RoomId);
            }
            else if (!isEditMode && roomDisplay != null)
            {
                RoomId = roomDisplay.RoomId;
                LoadRoomInfo(roomDisplay.RoomId);
            }
            
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void LoadFacilities()
        {
            using (var db = new HotelManagementDbContext())
            {
                var facilities = db.Facilities.ToList();
                Facilities = new ObservableCollection<Facility>(facilities);
            }
        }

        private void LoadRoomInfo(int roomId)
        {
            using (var db = new HotelManagementDbContext())
            {
                var room = db.Rooms.Include(r => r.RoomType).FirstOrDefault(r => r.RoomId == roomId);
                if (room != null)
                {
                    RoomInfo = $"Phòng {room.RoomNumber} - {room.RoomType?.TypeName}";
                }
            }
        }

        public void ValidateQuantity()
        {
            ClearErrors(nameof(Quantity));
            if (!Quantity.HasValue || Quantity.Value <= 0)
            {
                AddError(nameof(Quantity), "Số lượng phải lớn hơn 0!");
            }
        }
        public void ValidateSelectedFacility()
        {
            ClearErrors(nameof(SelectedFacility));
            if (SelectedFacility == null)
            {
                AddError(nameof(SelectedFacility), "Vui lòng chọn thiết bị!");
            }
        }

        private void Save()
        {
            ValidateSelectedFacility();
            ValidateQuantity();
            if (HasErrors)
            {
                return;
            }
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var rf = db.RoomFacilities.Find(RoomFacilityId);
                    if (rf != null)
                    {
                        rf.FacilityId = FacilityId;
                        rf.Quantity = Quantity;
                        db.SaveChanges();
                        RoomFacilitySaved?.Invoke(rf);
                    }
                }
                else
                {
                    // Kiểm tra xem thiết bị đã tồn tại trong phòng chưa
                    var existingFacility = db.RoomFacilities
                        .FirstOrDefault(rf => rf.RoomId == RoomId && rf.FacilityId == FacilityId);
                    
                    if (existingFacility != null)
                    {
                        // Cập nhật số lượng nếu thiết bị đã tồn tại
                        existingFacility.Quantity = (existingFacility.Quantity ?? 0) + (Quantity ?? 0);
                        db.SaveChanges();
                        RoomFacilitySaved?.Invoke(existingFacility);
                    }
                    else
                    {
                        // Thêm mới thiết bị
                        var rf = new RoomFacility
                        {
                            RoomId = RoomId,
                            FacilityId = SelectedFacility.FacilityId,
                            Quantity = Quantity
                        };
                        db.RoomFacilities.Add(rf);
                        db.SaveChanges();
                        RoomFacilitySaved?.Invoke(rf);
                    }
                }
            }
            RequestClose?.Invoke();
        }
    }
} 