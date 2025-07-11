using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class AmenityManagementViewModel : ViewModelBase
    {
        public ObservableCollection<Amenity> AmenityList { get; set; } = new();
        public ObservableCollection<Amenity> FilteredAmenityList { get; set; } = new();

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { if (SetProperty(ref _searchKeyword, value)) FilterAmenities(); }
        }

        public ICommand EditCommand { get; }
        public ICommand AddCommand { get; }

        public AmenityManagementViewModel()
        {
            using (var db = new HotelManagementDbContext())
            {
                AmenityList = new ObservableCollection<Amenity>(db.Amenities.ToList());
            }
            FilteredAmenityList = new ObservableCollection<Amenity>(AmenityList);
            EditCommand = new RelayCommand(a => EditAmenity(a as Amenity));
            AddCommand = new RelayCommand(_ => AddAmenity());
        }

        private void FilterAmenities()
        {
            var keyword = SearchKeyword?.Trim().ToLower() ?? "";
            var query = AmenityList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(a =>
                    (a.AmenityName != null && a.AmenityName.ToLower().Contains(keyword))
                );
            }

            FilteredAmenityList.Clear();
            foreach (var a in query) FilteredAmenityList.Add(a);
        }

        private void EditAmenity(Amenity? amenity)
        {
            if (amenity == null) return;
            var editWindow = new AmenityEditWindow();
            var vm = new AmenityEditViewModel(amenity);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.AmenitySaved += updatedAmenity =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.Amenities.FirstOrDefault(a => a.AmenityId == updatedAmenity.AmenityId);
                    if (refreshed != null)
                    {
                        var idx = AmenityList.IndexOf(amenity);
                        if (idx >= 0) AmenityList[idx] = refreshed;
                        FilterAmenities();
                    }
                }
            };
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void AddAmenity()
        {
            var addWindow = new AmenityEditWindow();
            var vm = new AmenityEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.AmenitySaved += newAmenity =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.Amenities.FirstOrDefault(a => a.AmenityId == newAmenity.AmenityId);
                    if (refreshed != null)
                    {
                        AmenityList.Add(refreshed);
                        FilterAmenities();
                    }
                }
            };
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 