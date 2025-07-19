using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Views.Windows;
using HotelManagementSystem.Models;
using System.Windows;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class RoomManagementUserControl : UserControl
    {
        public RoomManagementUserControl(RoomManagementViewModel? mainVm = null)
        {
            InitializeComponent();
            if (mainVm == null)
                mainVm = new RoomManagementViewModel();
            this.DataContext = mainVm;
            
            System.Diagnostics.Debug.WriteLine("RoomManagementUserControl constructor - registering event handlers");
            
            // Event handlers cho Room CRUD
            mainVm.RequestAddEditRoom += (room) => 
            {
                System.Diagnostics.Debug.WriteLine($"RequestAddEditRoom event triggered. room: {(room == null ? "null" : room.RoomId.ToString())}");
                if (room == null)
                    OpenAddRoomWindow(mainVm);
                else
                    OpenEditRoomWindow(mainVm, room);
            };

            // Callback handlers cho RoomFacility CRUD
            mainVm.OnAddRoomFacilityRequested = (vm) => 
            {
                System.Diagnostics.Debug.WriteLine("OnAddRoomFacilityRequested callback triggered");
                OpenAddRoomFacilityWindow(vm);
            };
            
            mainVm.OnEditRoomFacilityRequested = (vm, roomFacility) => 
            {
                System.Diagnostics.Debug.WriteLine($"OnEditRoomFacilityRequested callback triggered. roomFacility: {roomFacility.RoomFacilityId}");
                OpenEditRoomFacilityWindow(vm, roomFacility);
            };
        }

        private void OpenAddRoomWindow(RoomManagementViewModel mainVm)
        {
            var vm = new RoomAddEditViewModel(false);
            var win = new RoomAddEditWindow(vm);
            vm.RoomSaved += _ => mainVm.LoadRooms();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }

        private void OpenEditRoomWindow(RoomManagementViewModel mainVm, Room room)
        {
            var vm = new RoomAddEditViewModel(true, room);
            var win = new RoomAddEditWindow(vm);
            vm.RoomSaved += _ => mainVm.LoadRooms();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }

        private void OpenAddRoomFacilityWindow(RoomManagementViewModel mainVm)
        {
            System.Diagnostics.Debug.WriteLine($"OpenAddFacilityWindow called. SelectedRoomDisplay: {(mainVm.SelectedRoomDisplay == null ? "null" : mainVm.SelectedRoomDisplay.RoomNumber)}");
            var vm = new RoomFacilityAddEditViewModel(false, null, mainVm.SelectedRoomDisplay);
            var win = new RoomFacilityAddEditWindow(vm);
            vm.RoomFacilitySaved += _ => mainVm.LoadRoomFacilities();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }

        private void OpenEditRoomFacilityWindow(RoomManagementViewModel mainVm, RoomFacility roomFacility)
        {
            var vm = new RoomFacilityAddEditViewModel(true, roomFacility);
            var win = new RoomFacilityAddEditWindow(vm);
            vm.RoomFacilitySaved += _ => mainVm.LoadRoomFacilities();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }
    }
} 