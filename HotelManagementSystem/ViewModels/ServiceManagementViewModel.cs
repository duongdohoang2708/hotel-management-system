using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;
using HotelManagementSystem.Views.Windows;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceManagementViewModel : ViewModelBase
    {
        public ObservableCollection<Service> ServiceList { get; set; } = new();
        public ObservableCollection<Service> FilteredServiceList { get; set; } = new();

        private string _searchKeyword = "";
        public string SearchKeyword
        {
            get => _searchKeyword;
            set { if (SetProperty(ref _searchKeyword, value)) FilterServices(); }
        }

        public ICommand EditCommand { get; }
        public ICommand AddCommand { get; }

        public ServiceManagementViewModel()
        {
            using (var db = new HotelManagementDbContext())
            {
                ServiceList = new ObservableCollection<Service>(db.Services.Include(s => s.Category).ToList());
            }
            FilteredServiceList = new ObservableCollection<Service>(ServiceList);
            EditCommand = new RelayCommand(s => EditService(s as Service));
            AddCommand = new RelayCommand(_ => AddService());
        }

        private void FilterServices()
        {
            var keyword = SearchKeyword?.Trim().ToLower() ?? "";
            var query = ServiceList.AsEnumerable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                query = query.Where(s =>
                    (s.ServiceName != null && s.ServiceName.ToLower().Contains(keyword)) ||
                    (s.Category != null && s.Category.CategoryName.ToLower().Contains(keyword))
                );
            }

            FilteredServiceList.Clear();
            foreach (var s in query) FilteredServiceList.Add(s);
        }

        private void EditService(Service? service)
        {
            if (service == null) return;
            var editWindow = new ServiceEditWindow();
            var vm = new ServiceEditViewModel(service);
            editWindow.DataContext = vm;
            vm.RequestClose += () => editWindow.Close();
            vm.ServiceSaved += updatedService =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.Services.Include(s => s.Category).FirstOrDefault(s => s.ServiceId == updatedService.ServiceId);
                    if (refreshed != null)
                    {
                        var idx = ServiceList.IndexOf(service);
                        if (idx >= 0) ServiceList[idx] = refreshed;
                        FilterServices();
                    }
                }
            };
            editWindow.Owner = System.Windows.Application.Current.MainWindow;
            editWindow.ShowDialog();
        }

        private void AddService()
        {
            var addWindow = new ServiceEditWindow();
            var vm = new ServiceEditViewModel();
            addWindow.DataContext = vm;
            vm.RequestClose += () => addWindow.Close();
            vm.ServiceSaved += newService =>
            {
                using (var db = new HotelManagementDbContext())
                {
                    var refreshed = db.Services.Include(s => s.Category).FirstOrDefault(s => s.ServiceId == newService.ServiceId);
                    if (refreshed != null)
                    {
                        ServiceList.Add(refreshed);
                        FilterServices();
                    }
                }
            };
            addWindow.Owner = System.Windows.Application.Current.MainWindow;
            addWindow.ShowDialog();
        }
    }
} 