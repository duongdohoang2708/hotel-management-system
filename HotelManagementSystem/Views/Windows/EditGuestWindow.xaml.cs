using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;
using System;
using System.Windows;

namespace HotelManagementSystem.Views.Windows
{
    public partial class EditGuestWindow : Window
    {
        public EditGuestWindow(Guest guest)
        {
            InitializeComponent();
            var vm = new EditGuestViewModel(guest);
            vm.RequestClose += () => this.Close();
            this.DataContext = vm;
        }
    }
} 