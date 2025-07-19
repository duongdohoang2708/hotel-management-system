using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using System.Windows;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class GuestManagementUserControl : UserControl
    {
        public GuestManagementUserControl()
        {
            InitializeComponent();
            var vm = new GuestManagementViewModel();
            this.DataContext = vm;
        }
    }
} 