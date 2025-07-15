using System.Windows.Controls;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class RoomFacilityManagementControl : UserControl
    {
        public RoomFacilityManagementControl()
        {
            InitializeComponent();
            this.DataContext = new RoomFacilityManagementViewModel();
        }
    }
} 