using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using HotelManagementSystem.Views.UserControls;
using HotelManagementSystem.ViewModels;

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

        private string _currentViewTitle;
        public string CurrentViewTitle
        {
            get => _currentViewTitle;
            set => SetProperty(ref _currentViewTitle, value);
        }
        
        public MainWindowViewModel()
        {
            CurrentView = new WelcomeUserControl();
            CurrentViewTitle = "Trang chủ";
        }
    }
}
