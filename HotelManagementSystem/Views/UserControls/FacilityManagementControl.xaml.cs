using System.Windows.Controls;
using HotelManagementSystem.ViewModels; 

namespace HotelManagementSystem.Views.UserControls
{
    public partial class FacilityManagementControl : UserControl
    {
        public FacilityManagementControl()
        {
            InitializeComponent();
            this.DataContext = new FacilityManagementViewModel();
        }
    }
} 