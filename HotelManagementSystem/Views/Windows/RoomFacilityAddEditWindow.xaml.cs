using System.Windows;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class RoomFacilityAddEditWindow : Window
    {
        public RoomFacilityAddEditWindow(RoomFacilityAddEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            
            viewModel.RequestClose += () => Close();
        }
    }
} 