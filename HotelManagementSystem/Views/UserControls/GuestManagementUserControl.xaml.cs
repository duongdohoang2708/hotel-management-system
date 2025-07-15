using HotelManagementSystem.ViewModels;
using System.Windows.Controls;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class GuestManagementUserControl : UserControl
    {
        public GuestManagementUserControl()
        {
            InitializeComponent();
            this.DataContext = new GuestManagementViewModel();
        }
    }
} 