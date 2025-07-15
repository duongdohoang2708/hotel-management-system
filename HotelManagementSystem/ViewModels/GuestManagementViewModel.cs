using HotelManagementSystem.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;

namespace HotelManagementSystem.ViewModels
{
    public class GuestManagementViewModel : ViewModelBase
    {
        // Thêm class phụ để hiển thị
        public class GuestDisplay : Guest
        {
        }

        private ObservableCollection<GuestDisplay> _guests;
        public ObservableCollection<GuestDisplay> Guests
        {
            get => _guests;
            set => SetProperty(ref _guests, value);
        }

        private ObservableCollection<GuestDisplay> _filteredGuests;
        public ObservableCollection<GuestDisplay> FilteredGuests
        {
            get => _filteredGuests;
            set => SetProperty(ref _filteredGuests, value);
        }

        private string _searchText;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                    FilterGuests();
            }
        }

        public ICommand EditGuestCommand { get; }

        public GuestManagementViewModel()
        {
            LoadGuests();
            EditGuestCommand = new RelayCommand(EditGuest);
        }

        private void LoadGuests()
        {
            using (var db = new HotelManagementDbContext())
            {
                Guests = new ObservableCollection<GuestDisplay>(
                    db.Guests.ToList().Select(g => new GuestDisplay
                    {
                        GuestId = g.GuestId,
                        FullName = g.FullName,
                        IdCardNo = g.IdCardNo,
                        Address = g.Address,
                        Phone = g.Phone
                    })
                );
            }
            FilteredGuests = new ObservableCollection<GuestDisplay>(Guests);
        }

        private void FilterGuests()
        {
            if (string.IsNullOrWhiteSpace(SearchText))
            {
                FilteredGuests = new ObservableCollection<GuestDisplay>(Guests);
            }
            else
            {
                var lower = SearchText.ToLower();
                FilteredGuests = new ObservableCollection<GuestDisplay>(
                    Guests.Where(g =>
                        (!string.IsNullOrEmpty(g.FullName) && g.FullName.ToLower().Contains(lower)) ||
                        (!string.IsNullOrEmpty(g.Phone) && g.Phone.Contains(SearchText))
                    ));
            }
        }

        private void EditGuest(object? parameter)
        {
            if (parameter is GuestDisplay guest)
            {
                var editWindow = new Views.Windows.EditGuestWindow(guest);
                if (editWindow.ShowDialog() == true)
                {
                    LoadGuests();
                }
            }
        }
    }
} 