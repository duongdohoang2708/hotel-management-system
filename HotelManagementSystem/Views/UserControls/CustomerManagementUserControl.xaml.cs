using HotelManagementSystem.ViewModels;
using System.Windows.Controls;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class CustomerManagementUserControl : UserControl
    {
        public CustomerManagementUserControl()
        {
            InitializeComponent();
            this.DataContext = new CustomerManagementViewModel();
        }
    }
} 