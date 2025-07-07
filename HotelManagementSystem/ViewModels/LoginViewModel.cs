using System.Windows.Input;
using HotelManagementSystem.Models;
using HotelManagementSystem.Services;

namespace HotelManagementSystem.ViewModels
{
    public class LoginViewModel : ViewModelBase
    {
        private string _username;
        private string _password;
        private string _errorMessage;
        private readonly AccountService _accountService;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public ICommand LoginCommand { get; }
        public ICommand OpenForgotPasswordCommand { get; }

        public event Action<Account>? LoginSuccess;
        public event Action? ForgotPasswordRequested;

        public LoginViewModel(AccountService accountService)
        {
            _accountService = accountService;
            LoginCommand = new RelayCommand(_ => Login());
            OpenForgotPasswordCommand = new RelayCommand(_ => OpenForgotPassword());
        }

        private void Login()
        {
            ErrorMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Username))
            {
                ErrorMessage = "Vui lòng nhập tên đăng nhập!";
                return;
            }
            if (string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Vui lòng nhập mật khẩu!";
                return;
            }
            var account = _accountService.Authenticate(Username, Password);
            if (account == null)
            {
                ErrorMessage = "Sai tài khoản hoặc mật khẩu!";
                return;
            }
            LoginSuccess?.Invoke(account);
        }

        private void OpenForgotPassword()
        {
            ForgotPasswordRequested?.Invoke();
        }
    }
} 