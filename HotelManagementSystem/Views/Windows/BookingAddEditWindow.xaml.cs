using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Models;
using System.Windows;

namespace HotelManagementSystem.Views.Windows
{
    public partial class BookingAddEditWindow : Window
    {
        public BookingAddEditWindow(BookingAddEditViewModel vm)
        {
            InitializeComponent();
            DataContext = vm;
            vm.BookingSaved += OnBookingSaved;
        }

        private void OnBookingSaved(Booking booking)
        {
            DialogResult = booking != null;
            Close();
        }
    }
} 