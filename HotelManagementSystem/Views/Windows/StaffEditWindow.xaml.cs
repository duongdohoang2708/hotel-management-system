using System.Windows;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class StaffEditWindow : Window
    {
        public StaffEditWindow(Staff? staff = null)
        {
            InitializeComponent();
            var vm = new StaffEditViewModel(staff);
            vm.RequestClose += () => this.Close();
            this.DataContext = vm;
        }
    }
} 