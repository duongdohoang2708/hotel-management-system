using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class RoomAmenityManagementViewModel : ViewModelBase
    {
        public ObservableCollection<RoomAmenity> RoomAmenityList { get; set; } = new();
        public ObservableCollection<RoomAmenity> FilteredRoomAmenityList { get; set; } = new();

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { if (SetProperty(ref _searchKeyword, value)) FilterRoomAmenities(); }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddCommand { get; }

        public RoomAmenityManagementViewModel()
        {
            using (var db = new HotelManagementDbContext())
            {
                RoomAmenityList = new ObservableCollection<RoomAmenity>(
                    db.RoomAmenities.Include(ra => ra.Room).Include(ra => ra.Amenity).ToList()
                );
            }
            FilteredRoomAmenityList = new ObservableCollection<RoomAmenity>(RoomAmenityList);
            EditCommand = new RelayCommand(ra => EditRoomAmenity(ra as RoomAmenity));
            DeleteCommand = new RelayCommand(ra => DeleteRoomAmenity(ra as RoomAmenity));
            AddCommand = new RelayCommand(_ => AddRoomAmenity());
        }

        private void FilterRoomAmenities()
        {
            var keyword = SearchKeyword?.Trim().ToLower() ?? "";
            var query = RoomAmenityList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(ra =>
                    (ra.Room != null && ra.Room.RoomNumber.ToLower().Contains(keyword)) ||
                    (ra.Amenity != null && ra.Amenity.AmenityName.ToLower().Contains(keyword))
                );
            }

            FilteredRoomAmenityList.Clear();
            foreach (var ra in query) FilteredRoomAmenityList.Add(ra);
        }

        private void EditRoomAmenity(RoomAmenity? roomAmenity)
        {
            if (roomAmenity == null) return;
            var editWindow = new RoomAmenityEditWindow();
            var vm = new RoomAmenityEditViewModel(roomAmenity);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.RoomAmenitySaved += updatedRA =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.RoomAmenities.Include(ra => ra.Room).Include(ra => ra.Amenity)
                        .FirstOrDefault(ra => ra.RoomId == updatedRA.RoomId && ra.AmenityId == updatedRA.AmenityId);
                    if (refreshed != null)
                    {
                        var idx = RoomAmenityList.IndexOf(roomAmenity);
                        if (idx >= 0) RoomAmenityList[idx] = refreshed;
                        FilterRoomAmenities();
                    }
                }
            };
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void DeleteRoomAmenity(RoomAmenity? roomAmenity)
        {
            if (roomAmenity == null) return;
            var result = System.Windows.MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tiện nghi '{roomAmenity.Amenity?.AmenityName}' khỏi phòng {roomAmenity.Room?.RoomNumber}?",
                "Xác nhận xóa",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question
            );
            if (result != System.Windows.MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbRA = db.RoomAmenities.FirstOrDefault(ra => ra.RoomId == roomAmenity.RoomId && ra.AmenityId == roomAmenity.AmenityId);
                if (dbRA != null)
                {
                    db.RoomAmenities.Remove(dbRA);
                    db.SaveChanges();
                }
            }
            RoomAmenityList.Remove(roomAmenity);
            FilterRoomAmenities();
        }

        private void AddRoomAmenity()
        {
            var addWindow = new RoomAmenityEditWindow();
            var vm = new RoomAmenityEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.RoomAmenitySaved += newRA =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.RoomAmenities.Include(ra => ra.Room).Include(ra => ra.Amenity)
                        .FirstOrDefault(ra => ra.RoomId == newRA.RoomId && ra.AmenityId == newRA.AmenityId);
                    if (refreshed != null)
                    {
                        RoomAmenityList.Add(refreshed);
                        FilterRoomAmenities();
                    }
                }
            };
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 