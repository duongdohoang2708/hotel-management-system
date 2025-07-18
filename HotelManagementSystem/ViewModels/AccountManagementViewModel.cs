using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;


namespace HotelManagementSystem.ViewModels
{
    class AccountManagementViewModel : ViewModelBase
    {
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }

        //public event Action? AddAccountRequested;
        //public event Action<AccountDisplay>? EditAccountRequested;

        public event Action<Account?>? RequestAddEditAccount;
        private AccountDisplay _selectedAccount;
        public AccountDisplay SelectedAccount
        {
            get => _selectedAccount;
            set => _selectedAccount = value;
        }

        public class AccountDisplay
        {
            public int AccountID { get; set; }
            public string Username { get; set; }
            public string Password { get; set; }
            public string Role { get; set; }
            public string FullName { get; set; }
            public string Position { get; set; }
        }

        private ObservableCollection<AccountDisplay> _accountList;
        public ObservableCollection<AccountDisplay> AccountList
        {
            get => _accountList;
            set => SetProperty(ref _accountList, value);
        }

        public AccountManagementViewModel()
        {
            AddAccountCommand = new RelayCommand(_ => RequestAddEditAccount?.Invoke(null));
            EditAccountCommand = new RelayCommand(param =>
            {
                var acc = param as AccountDisplay;
                if (acc != null)
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        var account = db.Accounts.FirstOrDefault(a => a.AccountId == acc.AccountID);
                        if (account != null)
                            RequestAddEditAccount?.Invoke(account);
                    }
                }
            });
            DeleteAccountCommand = new RelayCommand(param => DeleteAccount(param as AccountDisplay));
            LoadAccounts();
        }
        
        private void DeleteAccount(AccountDisplay? acc)
        {
            if (acc == null) return;
            var result = System.Windows.MessageBox.Show($"Bạn có chắc chắn muốn xóa tài khoản '{acc.Username}'?", "Xác nhận xóa", System.Windows.MessageBoxButton.YesNo, System.Windows.MessageBoxImage.Question);
            if (result != System.Windows.MessageBoxResult.Yes)
                return;
            using (var db = new HotelManagementDbContext())
            {
                var account = db.Accounts.FirstOrDefault(a => a.AccountId == acc.AccountID);
                if (account != null)
                {
                    db.Accounts.Remove(account);
                    db.SaveChanges();
                }
            }
            LoadAccounts();
        }

        public void LoadAccounts()
        {
            using (var db = new HotelManagementDbContext())
            {
                var accounts = db.Accounts.Include(a => a.Staff).ToList();
                AccountList = new ObservableCollection<AccountDisplay>(
                    accounts.Select(a => new AccountDisplay
                    {
                        AccountID = a.AccountId,
                        Username = a.Username,
                        Password = a.Password,
                        Role = a.Role,
                        FullName = a.Staff?.FullName ?? "",
                        Position = a.Staff?.Position ?? ""
                    })
                );
            }
        }
    }
}
