using System.Windows;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class RoomTypeAddEditWindow : Window
    {
        public RoomTypeAddEditWindow(RoomTypeAddEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += () => this.Close();
        }
    }
} 