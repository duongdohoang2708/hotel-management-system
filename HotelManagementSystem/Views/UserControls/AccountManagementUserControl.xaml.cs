using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.Views.UserControls
{
    /// <summary>
    /// Interaction logic for UC_AccountManagement.xaml
    /// </summary>
    public partial class AccountManagementUserControl : UserControl
    {
        public AccountManagementUserControl()
        {
            InitializeComponent();
            var vm = new AccountManagementViewModel();
            this.DataContext = vm;
            if (vm is AccountManagementViewModel mainVm)
            {
                //mainVm.AddAccountRequested += () => OpenAddAccountWindow(mainVm);
                //mainVm.EditAccountRequested += (acc) => OpenEditAccountWindow(mainVm, acc);
            }
        }

        private void OpenAddAccountWindow(AccountManagementViewModel mainVm)
        {
            var vm = new AccountAddEditViewModel(false);
            var win = new AccountAddEditWindow(vm);
            vm.AccountSaved += _ => mainVm.LoadAccounts();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }

        private void OpenEditAccountWindow(AccountManagementViewModel mainVm, AccountManagementViewModel.AccountDisplay acc)
        {
            var dbAcc = new HotelManagementSystem.Models.Account
            {
                AccountId = acc.AccountID,
                Username = acc.Username,
                Password = acc.Password,
                Role = acc.Role,
                StaffId = 0 // Không sửa StaffId
            };
            var vm = new AccountAddEditViewModel(true, dbAcc);
            var win = new AccountAddEditWindow(vm);
            vm.AccountSaved += _ => mainVm.LoadAccounts();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }
    }
}
