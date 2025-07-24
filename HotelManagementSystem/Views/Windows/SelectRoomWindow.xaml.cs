using System.Collections.ObjectModel;
using System.Windows;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Views.Windows
{
    public partial class SelectRoomWindow : Window
    {
        public ObservableCollection<RoomDisplay> Rooms { get; set; }
        public RoomDisplay SelectedRoom { get; set; }

        public SelectRoomWindow(ObservableCollection<RoomDisplay> rooms)
        {
            InitializeComponent();
            Rooms = rooms;
            DataContext = this;
        }

        private void SelectButton_Click(object sender, RoutedEventArgs e)
        {
            if (SelectedRoom != null)
            {
                DialogResult = true;
                Close();
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = false;
            Close();
        }
    }
} 