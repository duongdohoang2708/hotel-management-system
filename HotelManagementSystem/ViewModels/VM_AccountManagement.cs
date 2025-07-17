using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace HotelManagementSystem.ViewModels
{
    class VM_AccountManagement
    {
        public ICommand AddAccountCommand { get; }
        public ICommand EditAccountCommand { get; }
        public ICommand DeleteAccountCommand { get; }

        public VM_AccountManagement()
        {
            AddAccountCommand = new RelayCommand(_ => AddAccount());
            EditAccountCommand = new RelayCommand(_ => EditAccount());
            DeleteAccountCommand = new RelayCommand(_ => DeleteAccount());
        }
        private void AddAccount()
        {
            // Logic to add a new account
            Console.WriteLine("Add Account Clicked");
        }
        private void EditAccount()
        {
            // Logic to edit an existing account
            Console.WriteLine("Edit Account Clicked");
        }
        private void DeleteAccount()
        {
            // Logic to delete an account
            Console.WriteLine("Delete Account Clicked");
        }
    }
}
