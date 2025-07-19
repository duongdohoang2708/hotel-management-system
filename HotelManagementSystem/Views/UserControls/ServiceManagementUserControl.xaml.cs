using System.Windows.Controls;
using HotelManagementSystem.ViewModels;
using HotelManagementSystem.Views.Windows;
using HotelManagementSystem.Models;
using System.Windows;
using System.Linq;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.Views.UserControls
{
    public partial class ServiceManagementUserControl : UserControl
    {
        public ServiceManagementUserControl()
        {
            InitializeComponent();
            var vm = new ServiceManagementViewModel();
            this.DataContext = vm;
            if (vm is ServiceManagementViewModel mainVm)
            {
                mainVm.RequestAddEditService += (service) => 
                {
                    if (service == null)
                        OpenAddServiceWindow(mainVm);
                    else
                        OpenEditServiceWindow(mainVm, service);
                };
            }
        }

        private void OpenAddServiceWindow(ServiceManagementViewModel mainVm)
        {
            var vm = new ServiceAddEditViewModel(false);
            var win = new ServiceAddEditWindow(vm);
            vm.ServiceSaved += _ => mainVm.LoadServices();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }

        private void OpenEditServiceWindow(ServiceManagementViewModel mainVm, Service service)
        {
            var vm = new ServiceAddEditViewModel(true, service);
            var win = new ServiceAddEditWindow(vm);
            vm.ServiceSaved += _ => mainVm.LoadServices();
            win.Owner = Application.Current.MainWindow;
            win.ShowDialog();
        }
    }
} 