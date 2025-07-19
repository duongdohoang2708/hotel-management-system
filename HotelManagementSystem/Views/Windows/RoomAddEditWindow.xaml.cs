using System.Windows;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class RoomAddEditWindow : Window
    {
        public RoomAddEditWindow(RoomAddEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            viewModel.RequestClose += () => Close();
        }
    }
} 