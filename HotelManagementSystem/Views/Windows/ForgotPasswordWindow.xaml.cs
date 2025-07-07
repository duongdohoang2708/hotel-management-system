using System.Windows;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;

namespace HotelManagementSystem.Views.Windows
{
    public partial class ForgotPasswordWindow : Window
    {
        public ForgotPasswordWindow()
        {
            InitializeComponent();

            var dbContext = new HotelManagementDbContext();
            var accountService = new AccountService(dbContext);
            var viewModel = new ForgotPasswordViewModel(accountService);
            DataContext = viewModel;

            NewPasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.NewPassword = NewPasswordBox.Password;
            };
            ConfirmPasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.ConfirmPassword = ConfirmPasswordBox.Password;
            };

            viewModel.PasswordChangedSuccess += () =>
            {

            };
        }
    }
} 