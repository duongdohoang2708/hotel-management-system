using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using System.Windows;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class RoomTypeManagementUserControl : UserControl
    {
        public RoomTypeManagementUserControl()
        {
            InitializeComponent();
            var vm = new RoomTypeManagementViewModel();
            this.DataContext = vm;
        }
    }
} 