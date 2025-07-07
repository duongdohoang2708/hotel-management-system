using System.Windows;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;

namespace HotelManagementSystem.Views.Windows
{
    public partial class LoginWindow : Window
    {
        public LoginWindow()
        {
            InitializeComponent();

            var dbContext = new HotelManagementDbContext();
            var accountService = new AccountService(dbContext);
            var viewModel = new LoginViewModel(accountService);

            DataContext = viewModel;

            PasswordBox.PasswordChanged += (s, e) =>
            {
                viewModel.Password = PasswordBox.Password;
            };

            viewModel.LoginSuccess += account =>
            {
                AppSession.CurrentAccount = account;
                DialogResult = true;
            };

            viewModel.ForgotPasswordRequested += () =>
            { 
                var forgotPasswordWindow = new ForgotPasswordWindow();
                forgotPasswordWindow.ShowDialog();
            };
        }
    }
} 