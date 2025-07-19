using System.Windows;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class FacilityAddEditWindow : Window
    {
        public FacilityAddEditWindow(FacilityAddEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += () => this.Close();
        }

        private void FacilityName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is FacilityAddEditViewModel vm)
                vm.ValidateFacilityName();
        }
    }
} 