using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class RoomManagementViewModel : ViewModelBase
    {
        public ObservableCollection<Room> RoomList { get; set; } = new();
        public ObservableCollection<Room> FilteredRoomList { get; set; } = new();

        private string _searchRoomNumber = "";
        public string SearchRoomNumber
        {
            get => _searchRoomNumber;
            set { if (SetProperty(ref _searchRoomNumber, value)) FilterRooms(); }
        }

        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }
        public ICommand AddCommand { get; }

        public RoomManagementViewModel()
        {
            using (var db = new HotelManagementDbContext())
            {
                RoomList = new ObservableCollection<Room>(db.Rooms.Include(r => r.RoomType).ToList());
            }
            FilteredRoomList = new ObservableCollection<Room>(RoomList);
            EditCommand = new RelayCommand(r => EditRoom(r as Room));
            DeleteCommand = new RelayCommand(r => DeleteRoom(r as Room));
            AddCommand = new RelayCommand(_ => AddRoom());
        }

        private void FilterRooms()
        {
            var keyword = SearchRoomNumber?.Trim().ToLower() ?? "";
            var query = RoomList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(r =>
                    (r.RoomNumber != null && r.RoomNumber.ToLower().Contains(keyword)) ||
                    (r.RoomType != null && r.RoomType.TypeName.ToLower().Contains(keyword)) ||
                    (r.Status != null && r.Status.ToLower().Contains(keyword)) ||
                    (r.CleanStatus != null && r.CleanStatus.ToLower().Contains(keyword))
                );
            }

            FilteredRoomList.Clear();
            foreach (var r in query) FilteredRoomList.Add(r);
        }

        private void EditRoom(Room? room)
        {
            if (room == null) return;
            var editWindow = new RoomEditWindow();
            var vm = new RoomEditViewModel(room);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.RoomSaved += updatedRoom =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.Rooms.Include(r => r.RoomType).FirstOrDefault(r => r.RoomId == updatedRoom.RoomId);
                    if (refreshed != null)
                    {
                        var idx = RoomList.IndexOf(room);
                        if (idx >= 0) RoomList[idx] = refreshed;
                        FilterRooms();
                    }
                }
            };
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void DeleteRoom(Room? room)
        {
            if (room == null) return;
            if (room.Status != "Phòng trống")
            {
                System.Windows.MessageBox.Show("Chỉ xóa được phòng khi phòng ở trạng thái 'phòng trống'!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return;
            }
            var result = System.Windows.MessageBox.Show(
                $"Bạn có chắc chắn muốn xóa phòng {room.RoomNumber}?", 
                "Xác nhận xóa", 
                System.Windows.MessageBoxButton.YesNo, 
                System.Windows.MessageBoxImage.Question
            );
            if (result != System.Windows.MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var dbRoom = db.Rooms.FirstOrDefault(r => r.RoomId == room.RoomId);
                if (dbRoom != null)
                {
                    db.Rooms.Remove(dbRoom);
                    db.SaveChanges();
                }
            }
            RoomList.Remove(room);
            FilterRooms();
        }

        private void AddRoom()
        {
            var addWindow = new RoomEditWindow();
            var vm = new RoomEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.RoomSaved += newRoom =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.Rooms.Include(r => r.RoomType).FirstOrDefault(r => r.RoomId == newRoom.RoomId);
                    if (refreshed != null)
                    {
                        RoomList.Add(refreshed);
                        FilterRooms();
                    }
                }
            };
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 