using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace HotelManagementSystem.ViewModels
{
    public class CustomerManagementViewModel : ViewModelBase
    {
        // Thêm class phụ để hiển thị Nam/Nữ
        public class CustomerDisplay : Customer
        {
            public string GenderDisplay => Gender == "M" ? "Nam" : (Gender == "F" ? "Nữ" : Gender);
        }

        private ObservableCollection<CustomerDisplay> _customers;
        public ObservableCollection<CustomerDisplay> Customers
        {
            get => _customers;
            set => SetProperty(ref _customers, value);
        }

        private ObservableCollection<CustomerDisplay> _filteredCustomers;
        public ObservableCollection<CustomerDisplay> FilteredCustomers
        {
            get => _filteredCustomers;
            set => SetProperty(ref _filteredCustomers, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    FilterCustomers();
            }
        }

        public ICommand EditCustomerCommand { get; }

        public CustomerManagementViewModel()
        {
            LoadCustomers();
            EditCustomerCommand = new RelayCommand(EditCustomer);
        }

        private void LoadCustomers()
        {
            using (var db = new HotelManagementDbContext())
            {
                Customers = new ObservableCollection<CustomerDisplay>(
                    db.Customers.ToList().Select(c => new CustomerDisplay
                    {
                        CustomerId = c.CustomerId,
                        FullName = c.FullName,
                        Gender = c.Gender,
                        Dob = c.Dob,
                        IdCardNo = c.IdCardNo,
                        Address = c.Address,
                        Phone = c.Phone
                    })
                );
            }
            FilteredCustomers = new ObservableCollection<CustomerDisplay>(Customers);
        }

        private void FilterCustomers()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredCustomers = new ObservableCollection<CustomerDisplay>(Customers);
            }
            else
            {
                var lower = SearchText.ToLower();
                FilteredCustomers = new ObservableCollection<CustomerDisplay>(
                    Customers.Where(c =>
                        (!string.IsNullOrEmpty(c.FullName) && c.FullName.ToLower().Contains(lower)) ||
                        (!string.IsNullOrEmpty(c.Phone) && c.Phone.Contains(SearchText)) ||
                        (!string.IsNullOrEmpty(c.GenderDisplay) && c.GenderDisplay.ToLower().Contains(lower))
                    ));
            }
        }

        private void EditCustomer(object? parameter)
        {
            if (parameter is CustomerDisplay customer)
            {
                var editWindow = new Views.Windows.EditCustomerWindow(customer);
                if (editWindow.ShowDialog() == true)
                {
                    LoadCustomers();
                }
            }
        }
    }
} 