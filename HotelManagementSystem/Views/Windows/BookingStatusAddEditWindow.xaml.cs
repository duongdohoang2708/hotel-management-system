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
    }
} 