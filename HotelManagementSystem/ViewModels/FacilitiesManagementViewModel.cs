using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;

namespace HotelManagementSystem.ViewModels
{
    public class FacilitiesManagementViewModel : ViewModelBase
    {
        public ICommand AddFacilityCommand { get; }
        public ICommand EditFacilityCommand { get; }
        public ICommand DeleteFacilityCommand { get; }

        public event Action<Facility?>? RequestAddEditFacility;

        private FacilityDisplay _selectedFacility;
        public FacilityDisplay SelectedFacility
        {
            get => _selectedFacility;
            set => SetProperty(ref _selectedFacility, value);
        }

        public class FacilityDisplay
        {
            public int FacilityId { get; set; }
            public string FacilityName { get; set; }
            public string Description { get; set; }
        }

        private ObservableCollection<FacilityDisplay> _facilityList;
        public ObservableCollection<FacilityDisplay> FacilityList
        {
            get => _facilityList;
            set => SetProperty(ref _facilityList, value);
        }

        public FacilitiesManagementViewModel()
        {
            AddFacilityCommand = new RelayCommand(_ => RequestAddEditFacility?.Invoke(null));
            EditFacilityCommand = new RelayCommand(param =>
            {
                var facility = param as FacilityDisplay;
                if (facility != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var dbFacility = db.Facilities.FirstOrDefault(f => f.FacilityId == facility.FacilityId);
                        if (dbFacility != null)
                            RequestAddEditFacility?.Invoke(dbFacility);
                    }
                }
            });
            DeleteFacilityCommand = new RelayCommand(param => DeleteFacility(param as FacilityDisplay));
            LoadFacilities();
        }

        public void LoadFacilities()
        {
            using (var db = new HotelManagementDbContext())
            {
                var facilities = db.Facilities.ToList();
                FacilityList = new ObservableCollection<FacilityDisplay>(
                    facilities.Select(f => new FacilityDisplay
                    {
                        FacilityId = f.FacilityId,
                        FacilityName = f.FacilityName,
                        Description = f.Description
                    })
                );
            }
        }

        private void DeleteFacility(FacilityDisplay facility)
        {
            if (facility == null) return;
            var result = MessageBox.Show($"Bạn có chắc chắn muốn xóa thiết bị '{facility.FacilityName}'?", "Xác nhận xóa", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbFacility = db.Facilities.FirstOrDefault(f => f.FacilityId == facility.FacilityId);
                if (dbFacility != null)
                {
                    db.Facilities.Remove(dbFacility);
                    db.SaveChanges();
                }
            }
            LoadFacilities();
        }
    }
} 