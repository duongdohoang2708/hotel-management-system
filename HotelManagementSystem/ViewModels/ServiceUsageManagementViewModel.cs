using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Input;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class ServiceUsageManagementViewModel : ViewModelBase
    {
        private readonly HotelManagementDbContext _dbContext;
        public ObservableCollection<RoomDisplay> OccupiedRooms { get; set; } = new ObservableCollection<RoomDisplay>();
        public ObservableCollection<ServiceUsageDisplay> RoomServiceUsages { get; set; } = new ObservableCollection<ServiceUsageDisplay>();
        public ObservableCollection<Service> AvailableServices { get; set; } = new ObservableCollection<Service>();

        private RoomDisplay _selectedRoom;
        public RoomDisplay SelectedRoom
        {
            get => _selectedRoom;
            set
            {
                if (SetProperty(ref _selectedRoom, value))
                {
                    LoadRoomServiceUsages();
                }
            }
        }

        private Service _selectedService;
        public Service SelectedService
        {
            get => _selectedService;
            set => SetProperty(ref _selectedService, value);
        }

        private int _serviceQuantity = 1;
        public int ServiceQuantity
        {
            get => _serviceQuantity;
            set => SetProperty(ref _serviceQuantity, value);
        }

        public ICommand AddServiceUsageCommand { get; }

        public ServiceUsageManagementViewModel()
        {
            _dbContext = new HotelManagementDbContext();
            LoadOccupiedRooms();
            LoadAvailableServices();
            AddServiceUsageCommand = new RelayCommand(_ => AddServiceUsage(), _ => SelectedRoom != null && SelectedService != null && ServiceQuantity > 0);
        }

        public class RoomDisplay
        {
            public int BookedRoomId { get; set; }
            public string RoomNumber { get; set; }
            public string RoomTypeName { get; set; }
            public string GuestName { get; set; }
        }

        public class ServiceUsageDisplay
        {
            public int UsageId { get; set; }
            public string RoomNumber { get; set; }
            public string ServiceName { get; set; }
            public decimal UnitPrice { get; set; }
            public DateTime UsageTime { get; set; }
        }

        private void LoadOccupiedRooms()
        {
            OccupiedRooms.Clear();
            var rooms = _dbContext.BookedRooms
                .Where(br => br.Booking.StatusId == 2)
                .Select(br => new RoomDisplay
                {
                    BookedRoomId = br.BookedRoomId,
                    RoomNumber = br.Room.RoomNumber,
                    RoomTypeName = br.Room.RoomType.TypeName,
                    GuestName = br.Booking.Guest.FullName
                }).ToList();
            foreach (var r in rooms)
                OccupiedRooms.Add(r);
        }

        private void LoadRoomServiceUsages()
        {
            RoomServiceUsages.Clear();
            if (SelectedRoom == null) return;
            var usages = _dbContext.RoomServiceUsages
                .Where(u => u.BookedRoomId == SelectedRoom.BookedRoomId)
                .Select(u => new ServiceUsageDisplay
                {
                    UsageId = u.UsageId,
                    RoomNumber = u.BookedRoom.Room.RoomNumber,
                    ServiceName = u.Service.ServiceName,
                    UnitPrice = u.UnitPrice ?? 0,
                    UsageTime = u.UsageTime ?? DateTime.MinValue
                }).ToList();
            foreach (var u in usages)
                RoomServiceUsages.Add(u);
        }

        private void LoadAvailableServices()
        {
            AvailableServices.Clear();
            var services = _dbContext.Services.ToList();
            foreach (var s in services)
                AvailableServices.Add(s);
        }

        private void AddServiceUsage()
        {
            if (SelectedRoom == null || SelectedService == null || ServiceQuantity <= 0) return;
            try
            {
                for (int i = 0; i < ServiceQuantity; i++)
                {
                    var usage = new RoomServiceUsage
                    {
                        BookedRoomId = SelectedRoom.BookedRoomId,
                        ServiceId = SelectedService.ServiceId,
                        UnitPrice = SelectedService.UnitPrice,
                        UsageTime = DateTime.Now
                    };
                    _dbContext.RoomServiceUsages.Add(usage);
                }
                _dbContext.SaveChanges();
                MessageBox.Show("Thêm dịch vụ thành công!", "Thông báo", MessageBoxButton.OK, MessageBoxImage.Information);
                LoadRoomServiceUsages();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Lỗi khi thêm dịch vụ: {ex.Message}", "Lỗi", MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
} 