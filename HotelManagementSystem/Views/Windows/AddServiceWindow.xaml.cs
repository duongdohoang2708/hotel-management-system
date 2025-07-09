using System.Windows;

namespace HotelManagementSystem.Views.Windows
{
    public partial class AddServiceWindow : Window
    {
        public AddServiceWindow()
        {
            InitializeComponent();
            this.Loaded += AddServiceWindow_Loaded;
        }

        private void AddServiceWindow_Loaded(object sender, RoutedEventArgs e)
        {
            if (this.DataContext is ViewModels.AddServiceViewModel vm)
            {
                vm.RequestClose += () => this.Close();
            }
        }
    }
} 