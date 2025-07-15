using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class FacilityManagementViewModel : ViewModelBase
    {
        public ObservableCollection<Facility> FacilityList { get; set; } = new();
        public ObservableCollection<Facility> FilteredFacilityList { get; set; } = new();

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { if (SetProperty(ref _searchKeyword, value)) FilterFacilities(); }
        }

        public ICommand EditCommand { get; }
        public ICommand AddCommand { get; }

        public FacilityManagementViewModel()
        {
            using (var db = new HotelManagementDbContext())
            {
                FacilityList = new ObservableCollection<Facility>(db.Facilities.ToList());
            }
            FilteredFacilityList = new ObservableCollection<Facility>(FacilityList);
            EditCommand = new RelayCommand(f => EditFacility(f as Facility));
            AddCommand = new RelayCommand(_ => AddFacility());
        }

        private void FilterFacilities()
        {
            var keyword = SearchKeyword?.Trim().ToLower() ?? "";
            var query = FacilityList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(f =>
                    (f.FacilityName != null && f.FacilityName.ToLower().Contains(keyword))
                );
            }

            FilteredFacilityList.Clear();
            foreach (var f in query) FilteredFacilityList.Add(f);
        }

        private void EditFacility(Facility? facility)
        {
            if (facility == null) return;
            var editWindow = new FacilityEditWindow();
            var vm = new FacilityEditViewModel(facility);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.FacilitySaved += updatedFacility =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.Facilities.FirstOrDefault(f => f.FacilityId == updatedFacility.FacilityId);
                    if (refreshed != null)
                    {
                        var idx = FacilityList.IndexOf(facility);
                        if (idx >= 0) FacilityList[idx] = refreshed;
                        FilterFacilities();
                    }
                }
            };
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void AddFacility()
        {
            var addWindow = new FacilityEditWindow();
            var vm = new FacilityEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.FacilitySaved += newFacility =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.Facilities.FirstOrDefault(f => f.FacilityId == newFacility.FacilityId);
                    if (refreshed != null)
                    {
                        FacilityList.Add(refreshed);
                        FilterFacilities();
                    }
                }
            };
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 