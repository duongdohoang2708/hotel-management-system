using System.Windows;
using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Converters;

namespace HotelManagementSystem.Views.Windows
{
    public partial class AccountAddEditWindow : Window
    {
        public AccountAddEditWindow(AccountAddEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += () => this.Close();
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountAddEditViewModel vm && sender is PasswordBox pb)
            {
                vm.Password = pb.Password;
            }
        }
        private void Username_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountAddEditViewModel vm)
                vm.ValidateUsername();
        }
        private void PasswordBox_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is AccountAddEditViewModel vm)
                vm.ValidatePassword();
        }
    }
} 