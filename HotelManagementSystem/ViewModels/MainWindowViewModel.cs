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
        public ICommand CustomerCommand { get; }
        public ICommand RoomManagementCommand { get; }
        public MainWindowViewModel()
        {
            CurrentView = new WelcomeUserControl();
            CustomerCommand = new RelayCommand(_ => CurrentView = new GuestManagementUserControl());
            RoomManagementCommand = new RelayCommand(_ => CurrentView = new RoomManagementControl());
        }
    }
}
