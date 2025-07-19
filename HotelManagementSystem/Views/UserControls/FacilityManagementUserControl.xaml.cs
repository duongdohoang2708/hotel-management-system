using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using System.Windows;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class FacilityManagementUserControl : UserControl
    {
        public FacilityManagementUserControl()
        {
            InitializeComponent();
            var vm = new FacilityManagementViewModel();
            this.DataContext = vm;
        }
    }
} 