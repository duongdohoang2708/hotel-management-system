using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using System;
using System.Windows;

namespace HotelManagementSystem.Views.Windows
{
    public partial class EditCustomerWindow : Window
    {
        public EditCustomerWindow(Customer customer)
        {
            InitializeComponent();
            var vm = new EditCustomerViewModel(customer);
            vm.RequestClose += () => this.DialogResult = true;
            this.DataContext = vm;
        }
    }
} 