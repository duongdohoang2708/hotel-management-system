using System.Windows;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class ServiceAddEditWindow : Window
    {
        public ServiceAddEditWindow(ServiceAddEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += () => this.Close();
        }
    }
} 