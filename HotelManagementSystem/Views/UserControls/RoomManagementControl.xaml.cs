using System.Windows.Controls;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class RoomManagementControl : UserControl
    {
        public RoomManagementControl()
        {
            InitializeComponent();
            this.DataContext = new RoomManagementViewModel();
        }
    }
} 