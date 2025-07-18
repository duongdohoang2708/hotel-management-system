using System;
using System.Windows.Input;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class FacilityAddEditViewModel : ViewModelBase
    {
        private int _facilityId;
        public int FacilityId
        {
            get => _facilityId;
            set => SetProperty(ref _facilityId, value);
        }

        private string _facilityName;
        public string FacilityName
        {
            get => _facilityName;
            set => SetProperty(ref _facilityName, value);
        }

        private string _description;
        public string Description
        {
            get => _description;
            set => SetProperty(ref _description, value);
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa thiết bị" : "Thêm thiết bị";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Facility>? FacilitySaved;

        public FacilityAddEditViewModel(bool isEditMode = false, Facility? facility = null)
        {
            IsEditMode = isEditMode;
            if (isEditMode && facility != null)
            {
                FacilityId = facility.FacilityId;
                FacilityName = facility.FacilityName;
                Description = facility.Description;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void Save()
        {
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var f = db.Facilities.Find(FacilityId);
                    if (f != null)
                    {
                        f.FacilityName = FacilityName;
                        f.Description = Description;
                        db.SaveChanges();
                        FacilitySaved?.Invoke(f);
                    }
                }
                else
                {
                    var f = new Facility
                    {
                        FacilityName = FacilityName,
                        Description = Description
                    };
                    db.Facilities.Add(f);
                    db.SaveChanges();
                    FacilitySaved?.Invoke(f);
                }
            }
            RequestClose?.Invoke();
        }
    }
} 