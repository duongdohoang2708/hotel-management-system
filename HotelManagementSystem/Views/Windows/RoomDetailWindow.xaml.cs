using System.Windows;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class RoomDetailWindow : Window
    {
        public event Action? RequestReloadRoomStatus;
        public RoomDetailWindow()
        {
            InitializeComponent();
            this.Loaded += RoomDetailWindow_Loaded;
        }

        private void RoomDetailWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is RoomDetailViewModel vm)
            {
                vm.RequestClose += () => this.Close();
                vm.RequestReloadRoomStatus += () => RequestReloadRoomStatus?.Invoke();
            }
        }
    }
} 