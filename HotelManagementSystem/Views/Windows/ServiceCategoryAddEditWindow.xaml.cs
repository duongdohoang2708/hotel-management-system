using System.Windows;
using HotelManagementSystem.ViewModels;

namespace HotelManagementSystem.Views.Windows
{
    public partial class ServiceCategoryAddEditWindow : Window
    {
        public ServiceCategoryAddEditWindow(ServiceCategoryAddEditViewModel viewModel)
        {
            InitializeComponent();
            DataContext = viewModel;
            viewModel.RequestClose += () => this.Close();
        }
    }
} 