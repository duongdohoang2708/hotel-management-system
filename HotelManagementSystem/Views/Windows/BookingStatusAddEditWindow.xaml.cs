using System.Windows;
using HotelManagementSystem.Models;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class BookingStatusAddEditWindow : Window
    {
        public BookingStatusAddEditWindow(BookingStatusAddEditViewModel viewModel)
        {
            InitializeComponent();
            viewModel.RequestClose += () => Close();
            DataContext = viewModel;
        }

        private void StatusName_LostFocus(object sender, RoutedEventArgs e)
        {
            if (DataContext is BookingStatusAddEditViewModel vm)
                vm.ValidateStatusName();
        }
    }
} 