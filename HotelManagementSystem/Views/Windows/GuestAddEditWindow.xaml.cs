using System.Windows;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class GuestAddEditWindow : Window
    {
        public GuestAddEditWindow(GuestAddEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += () => this.Close();
        }

        private void FullName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is GuestAddEditViewModel vm)
                vm.ValidateFullName();
        }
        private void Phone_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is GuestAddEditViewModel vm)
                vm.ValidatePhone();
        }
        private void IdCardNo_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is GuestAddEditViewModel vm)
                vm.ValidateIdCardNo();
        }
    }
} 