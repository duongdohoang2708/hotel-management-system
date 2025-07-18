using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using System.Windows;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class FacilitiesManagementUserControl : UserControl
    {
        public FacilitiesManagementUserControl()
        {
            InitializeComponent();
            var vm = new FacilitiesManagementViewModel();
            this.DataContext = vm;
        }
    }
} 