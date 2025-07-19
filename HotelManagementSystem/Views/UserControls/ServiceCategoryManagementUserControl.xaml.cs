using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using System.Windows;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class ServiceCategoryManagementUserControl : UserControl
    {
        public ServiceCategoryManagementUserControl()
        {
            InitializeComponent();
            var vm = new ServiceCategoryManagementViewModel();
            this.DataContext = vm;
        }
    }
} 