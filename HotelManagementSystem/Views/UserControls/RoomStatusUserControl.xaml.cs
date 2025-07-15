using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using System.Windows.Input;
using System.Windows.Media;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class RoomStatusUserControl : UserControl
    {
        public RoomStatusUserControl()
        {
            InitializeComponent();
            var viewModel = new RoomStatusViewModel();
            DataContext = viewModel;
        }

        private void CardBorder_MouseLeftButtonUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is Border border && border.DataContext is HotelManagementSystem.ViewModels.RoomStatusItem room)
            {
                if (DataContext is HotelManagementSystem.ViewModels.RoomStatusViewModel vm && vm.OpenRoomDetailCommand.CanExecute(room))
                {
                    vm.OpenRoomDetailCommand.Execute(room);
                }
            }
        }
    }
} 