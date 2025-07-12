using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using Microsoft.EntityFrameworkCore;

namespace HotelManagementSystem.ViewModels
{
    public class AddServiceViewModel : ViewModelBase
    {
        public ObservableCollection<ServiceCategory> ServiceCategoryList { get; }
        private ServiceCategory? _selectedCategory;
        public ServiceCategory? SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (SetProperty(ref _selectedCategory, value))
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        UpdateFilteredServiceList(db);
                    }
                }
            }
        }

        private string _searchText = string.Empty;
        public string SearchText
        {
            get => _searchText;
            set
            {
                if (SetProperty(ref _searchText, value))
                {
                    using (var db = new HotelManagementDbContext())
                    {
                        UpdateFilteredServiceList(db);
                    }
                }
            }
        }
        public ObservableCollection<Service> FilteredServiceList { get; } = new();
        public ObservableCollection<SelectedServiceItem> SelectedServices { get; } = new();
        public ICommand AddToSelectedCommand { get; }
        public ICommand RemoveSelectedCommand { get; }
        public ICommand ConfirmCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action? RequestReloadRoomDetail;

        private readonly int _roomId;
        private readonly int _reservationId;

        public AddServiceViewModel(int roomId, int reservationId)
        {
            _roomId = roomId;
            _reservationId = reservationId;
            using (var db = new HotelManagementDbContext())
            {
                ServiceCategoryList = new ObservableCollection<ServiceCategory>(db.ServiceCategories.ToList());
                SelectedCategory = ServiceCategoryList.FirstOrDefault();
                UpdateFilteredServiceList(db);
            }
            AddToSelectedCommand = new RelayCommand(s => AddToSelected(s as Service));
            RemoveSelectedCommand = new RelayCommand(s => RemoveSelected(s as SelectedServiceItem));
            ConfirmCommand = new RelayCommand(_ => OnConfirm(), _ => SelectedServices.Count > 0);
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void UpdateFilteredServiceList(HotelManagementDbContext db)
        {
            FilteredServiceList.Clear();
            var query = db.Services.Include(s => s.Category).AsQueryable();
            if (SelectedCategory != null)
                query = query.Where(s => s.CategoryId == SelectedCategory.CategoryId);
            if (!string.IsNullOrWhiteSpace(SearchText))
                query = query.Where(s => s.ServiceName.Contains(SearchText));
            foreach (var s in query.ToList())
                FilteredServiceList.Add(s);
        }

        private void AddToSelected(Service? service)
        {
            if (service == null) return;
            var exist = SelectedServices.FirstOrDefault(x => x.ServiceId == service.ServiceId);
            if (exist != null)
                exist.Quantity++;
            else
                SelectedServices.Add(new SelectedServiceItem
                {
                    ServiceId = service.ServiceId,
                    ServiceName = service.ServiceName,
                    UnitPrice = service.UnitPrice,
                    Quantity = 1
                });
            OnPropertyChanged(nameof(SelectedServices));
        }

        private void RemoveSelected(SelectedServiceItem? item)
        {
            if (item == null) return;
            SelectedServices.Remove(item);
            OnPropertyChanged(nameof(SelectedServices));
        }

        private void OnConfirm()
        {
            using (var db = new HotelManagementDbContext())
            {
                foreach (var item in SelectedServices)
                {
                    var rrs = new ReservationRoomService
                    {
                        ReservationId = _reservationId,
                        RoomId = _roomId,
                        ServiceId = item.ServiceId,
                        Qty = item.Quantity,
                        UnitPrice = item.UnitPrice
                    };
                    db.ReservationRoomServices.Add(rrs);
                }
                db.SaveChanges();
            }
            System.Windows.MessageBox.Show("Thêm dịch vụ thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestReloadRoomDetail?.Invoke();
            RequestClose?.Invoke();
        }
    }

    public class SelectedServiceItem : ViewModelBase
    {
        public int ServiceId { get; set; }
        public string ServiceName { get; set; } = string.Empty;
        public decimal UnitPrice { get; set; }
        private int _quantity = 1;
        public int Quantity
        {
            get => _quantity;
            set { if (SetProperty(ref _quantity, value)) OnPropertyChanged(nameof(TotalPrice)); }
        }
        public decimal TotalPrice => UnitPrice * Quantity;
    }
} 