using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class RoomFacilityManagementViewModel : ViewModelBase
    {
        public ObservableCollection<RoomFacility> RoomFacilityList { get; set; } = new();
        public ObservableCollection<RoomFacility> FilteredRoomFacilityList { get; set; } = new();

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { if (SetProperty(ref _searchKeyword, value)) FilterRoomFacilities(); }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddCommand { get; }

        public RoomFacilityManagementViewModel()
        {
            using (var db = new HotelManagementDbContext())
            {
                RoomFacilityList = new ObservableCollection<RoomFacility>(
                    db.RoomFacilities.Include(rf => rf.Room).Include(rf => rf.Facility).ToList()
                );
            }
            FilteredRoomFacilityList = new ObservableCollection<RoomFacility>(RoomFacilityList);
            EditCommand = new RelayCommand(rf => EditRoomFacility(rf as RoomFacility));
            DeleteCommand = new RelayCommand(rf => DeleteRoomFacility(rf as RoomFacility));
            AddCommand = new RelayCommand(_ => AddRoomFacility());
        }

        private void FilterRoomFacilities()
        {
            var keyword = SearchKeyword?.Trim().ToLower() ?? "";
            var query = RoomFacilityList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(rf =>
                    (rf.Room != null && rf.Room.RoomNumber.ToLower().Contains(keyword)) ||
                    (rf.Facility != null && rf.Facility.FacilityName.ToLower().Contains(keyword))
                );
            }

            FilteredRoomFacilityList.Clear();
            foreach (var rf in query) FilteredRoomFacilityList.Add(rf);
        }

        private void EditRoomFacility(RoomFacility? roomFacility)
        {
            if (roomFacility == null) return;
            var editWindow = new RoomFacilityEditWindow();
            var vm = new RoomFacilityEditViewModel(roomFacility);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.RoomFacilitySaved += updatedRF =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.RoomFacilities.Include(rf => rf.Room).Include(rf => rf.Facility)
                        .FirstOrDefault(rf => rf.RoomFacilityId == updatedRF.RoomFacilityId);
                    if (refreshed != null)
                    {
                        var idx = RoomFacilityList.IndexOf(roomFacility);
                        if (idx >= 0) RoomFacilityList[idx] = refreshed;
                        FilterRoomFacilities();
                    }
                }
            };
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void DeleteRoomFacility(RoomFacility? roomFacility)
        {
            if (roomFacility == null) return;
            var result = System.Windows.MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa tiện nghi '{roomFacility.Facility?.FacilityName}' khỏi phòng {roomFacility.Room?.RoomNumber}?",
                "Xác nhận xóa",
                System.Windows.MessageBoxButton.YesNo,
                System.Windows.MessageBoxImage.Question
            );
            if (result != System.Windows.MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbRF = db.RoomFacilities.FirstOrDefault(rf => rf.RoomId == roomFacility.RoomId && rf.FacilityId == roomFacility.FacilityId);
                if (dbRF != null)
                {
                    db.RoomFacilities.Remove(dbRF);
                    db.SaveChanges();
                }
            }
            RoomFacilityList.Remove(roomFacility);
            FilterRoomFacilities();
        }

        private void AddRoomFacility()
        {
            var addWindow = new RoomFacilityEditWindow();
            var vm = new RoomFacilityEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.RoomFacilitySaved += newRF =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.RoomFacilities.Include(rf => rf.Room).Include(rf => rf.Facility)
                        .FirstOrDefault(rf => rf.RoomId == newRF.RoomId && rf.FacilityId == newRF.FacilityId);
                    if (refreshed != null)
                    {
                        RoomFacilityList.Add(refreshed);
                        FilterRoomFacilities();
                    }
                }
            };
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 