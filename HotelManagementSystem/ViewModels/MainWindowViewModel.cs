using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManagementSystem.Views.UserControls;

namespace HotelManagementSystem.ViewModels
{
    public class MainWindowViewModel : ViewModelBase
    {
        private object _currentView;
        public object CurrentView
        {
            get => _currentView;
            set => SetProperty(ref _currentView, value);
        }
        public MainWindowViewModel()
        {
            CurrentView = new WelcomeUserControl();
        }
    }
}
