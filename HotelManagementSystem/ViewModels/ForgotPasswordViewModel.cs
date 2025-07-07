using HotelManagementSystem.Services;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotelManagementSystem.ViewModels
{
    public class ForgotPasswordViewModel : ViewModelBase
    {
        private string _username;
        private string _newPassword;
        private string _confirmPassword;
        private string _errorMessage;
        private string _successMessage;
        private readonly AccountService _accountService;

        public string Username
        {
            get => _username;
            set => SetProperty(ref _username, value);
        }

        public string NewPassword
        {
            get => _newPassword;
            set => SetProperty(ref _newPassword, value);
        }

        public string ConfirmPassword
        {
            get => _confirmPassword;
            set => SetProperty(ref _confirmPassword, value);
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set => SetProperty(ref _errorMessage, value);
        }

        public string SuccessMessage
        {
            get => _successMessage;
            set => SetProperty(ref _successMessage, value);
        }

        public ICommand ConfirmCommand { get; }

        public event Action? PasswordChangedSuccess;

        public ForgotPasswordViewModel(AccountService accountService)
        {
            _accountService = accountService;
            ConfirmCommand = new RelayCommand(_ => ChangePassword());
        }

        public void ChangePassword() 
        { 
            ErrorMessage = SuccessMessage = string.Empty;
            if (string.IsNullOrWhiteSpace(Username) || string.IsNullOrWhiteSpace(NewPassword) || string.IsNullOrWhiteSpace(ConfirmPassword)) 
            {
                ErrorMessage = "Vui lòng nhập đầy đủ thông tin!";
                return;
            }
            if (NewPassword != ConfirmPassword)
            {
                ErrorMessage = "Mật khẩu xác nhận không khớp!";
                return;
            }
            var result = _accountService.ChangePassword(Username, NewPassword);
            if (!result) 
            {
                ErrorMessage = "Tài khoản không tồn tại!";
                return;
            }
            SuccessMessage = "Đổi mật khẩu thành công!";
            PasswordChangedSuccess?.Invoke();
        }
    }
}
