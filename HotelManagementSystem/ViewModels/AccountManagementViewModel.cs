using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using System.Collections.ObjectModel;
using System.Windows.Threading;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;


namespace HotelManagementSystem.ViewModels
{
    class AccountManagementViewModel : ViewModelBase
    {
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }
        public ICommand SearchCommand { get; }
        public ICommand ClearSearchCommand { get; }

        //public event Action? AddAccountRequested;
        //public event Action<AccountDisplay>? EditAccountRequested;

        public event Action<Account?>? RequestAddEditAccount;
        private AccountDisplay _selectedAccount;
        public AccountDisplay SelectedAccount
        {
            get => _selectedAccount;
            set => _selectedAccount = value;
        }

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set 
            { 
                SetProperty(ref _searchKeyword, value);
                // Tìm kiếm tự động sau 500ms khi người dùng ngừng nhập
                _searchTimer?.Stop();
                _searchTimer?.Start();
            }
        }

        private DispatcherTimer _searchTimer;

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
            // Khởi tạo timer cho tìm kiếm tự động
            _searchTimer = new DispatcherTimer();
            _searchTimer.Interval = TimeSpan.FromMilliseconds(500);
            _searchTimer.Tick += (s, e) => 
            {
                _searchTimer.Stop();
                LoadAccounts();
            };

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
            SearchCommand = new RelayCommand(_ => LoadAccounts());
            ClearSearchCommand = new RelayCommand(_ => 
            {
                SearchKeyword = "";
                LoadAccounts();
            });
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
                var query = db.Accounts.Include(a => a.Staff).AsQueryable();
                
                // Áp dụng tìm kiếm nếu có từ khóa
                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    string keyword = SearchKeyword.Trim().ToLower();
                    query = query.Where(a => 
                        (a.Username != null && a.Username.ToLower().Contains(keyword)) ||
                        (a.Role != null && a.Role.ToLower().Contains(keyword)) ||
                        (a.Staff != null && a.Staff.FullName != null && a.Staff.FullName.ToLower().Contains(keyword)) ||
                        (a.Staff != null && a.Staff.Position != null && a.Staff.Position.ToLower().Contains(keyword))
                    );
                }

                var accounts = query.ToList();
                AccountList = new ObservableCollection<AccountDisplay>(
                    accounts.Select(a => new AccountDisplay
                    {
                        AccountID = a.AccountId,
                        Username = a.Username ?? "",
                        Password = a.Password ?? "",
                        Role = a.Role ?? "",
                        FullName = a.Staff?.FullName ?? "",
                        Position = a.Staff?.Position ?? ""
                    })
                );
            }
        }
    }
}
