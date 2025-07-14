using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Views.UserControls
{
    /// <summary>
    /// Interaction logic for SideBarUserControl.xaml
    /// </summary>
    public partial class SideBarUserControl : UserControl
    {
        public SideBarUserControl()
        {
            InitializeComponent();
        }

        private void TreeViewItem_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            if (sender is TreeViewItem item && item.DataContext is MenuItemModel menuItem && menuItem.Command != null && menuItem.Command.CanExecute(null))
            {
                menuItem.Command.Execute(null);
                e.Handled = true;
            }
        }
    }
}
