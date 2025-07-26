using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Input;
using HotelManagementSystem.Models;
using LiveCharts;
using LiveCharts.Wpf;

namespace HotelManagementSystem.ViewModels
{
    public class ReportDashboardViewModel : ViewModelBase
    {
        private readonly HotelManagementDbContext _dbContext;

        // Lọc theo thời gian
        private DateTime _fromDate = DateTime.Today.AddDays(-6);
        public DateTime FromDate
        {
            get => _fromDate;
            set { if (SetProperty(ref _fromDate, value)) LoadStatistics(); }
        }
        private DateTime _toDate = DateTime.Today;
        public DateTime ToDate
        {
            get => _toDate;
            set { if (SetProperty(ref _toDate, value)) LoadStatistics(); }
        }
        public ICommand ApplyFilterCommand { get; }

        // Chỉ số tổng quan
        public int TotalRooms { get; set; }
        public int AvailableRooms { get; set; }
        public int OccupiedRooms { get; set; }
        public int CleaningRooms { get; set; }
        public int BookedRooms { get; set; }
        public int TotalGuests { get; set; }
        public int TotalBookingsToday { get; set; }
        public int TotalBookingsThisMonth { get; set; }
        public decimal TotalRevenueToday { get; set; }
        public decimal TotalRevenueThisMonth { get; set; }
        public int TotalServiceUsages { get; set; }
        public int CurrentStayingGuests { get; set; }

        // Top phòng doanh thu cao nhất
        public ObservableCollection<TopRoomRevenue> TopRoomsByRevenue { get; set; } = new ObservableCollection<TopRoomRevenue>();
        // Top dịch vụ sử dụng nhiều nhất
        public ObservableCollection<TopServiceUsage> TopServicesByUsage { get; set; } = new ObservableCollection<TopServiceUsage>();

        // LiveCharts
        public SeriesCollection RevenueSeries { get; set; } = new SeriesCollection();
        public string[] RevenueLabels { get; set; } = Array.Empty<string>();
        public SeriesCollection RoomStatusSeries { get; set; } = new SeriesCollection();
        public SeriesCollection ServiceUsageSeries { get; set; } = new SeriesCollection();
        public string[] ServiceUsageLabels { get; set; } = Array.Empty<string>();

        public ReportDashboardViewModel()
        {
            _dbContext = new HotelManagementDbContext();
            ApplyFilterCommand = new RelayCommand(_ => LoadStatistics());
            LoadStatistics();
        }

        public class TopRoomRevenue
        {
            public string RoomNumber { get; set; }
            public decimal Revenue { get; set; }
        }
        public class TopServiceUsage
        {
            public string ServiceName { get; set; }
            public int UsageCount { get; set; }
        }

        private void LoadStatistics()
        {
            // Tổng số phòng
            TotalRooms = _dbContext.Rooms.Count();
            AvailableRooms = _dbContext.Rooms.Count(r => r.GetStatusByNow(_dbContext.Bookings) == "Trống");
            OccupiedRooms = _dbContext.Rooms.Count(r => r.GetStatusByNow(_dbContext.Bookings) == "Có khách");
            CleaningRooms = _dbContext.Rooms.Count(r => r.CleanStatus == "Chờ dọn");
            BookedRooms = _dbContext.Rooms.Count(r => r.GetStatusByNow(_dbContext.Bookings) == "Đã đặt");

            // Tổng số khách hàng
            TotalGuests = _dbContext.Guests.Count();

            // Tổng số booking trong ngày/tháng
            var today = DateTime.Today;
            var firstDayOfMonth = new DateTime(today.Year, today.Month, 1);
            TotalBookingsToday = _dbContext.Bookings.Count(b => b.BookingDate != null && b.BookingDate.Value.Date == today);
            TotalBookingsThisMonth = _dbContext.Bookings.Count(b => b.BookingDate != null && b.BookingDate.Value >= firstDayOfMonth);

            // Tổng doanh thu
            TotalRevenueToday = _dbContext.Invoices.Where(i => i.IssueDate != null && i.IssueDate.Value.Date == today).Sum(i => i.TotalAmount ?? 0);
            TotalRevenueThisMonth = _dbContext.Invoices.Where(i => i.IssueDate != null && i.IssueDate.Value >= firstDayOfMonth).Sum(i => i.TotalAmount ?? 0);

            // Tổng số dịch vụ đã sử dụng
            TotalServiceUsages = _dbContext.RoomServiceUsages.Count();

            // Số lượng khách đang lưu trú hiện tại
            CurrentStayingGuests = _dbContext.Bookings.Count(b => b.StatusId == 2);

            // Top 5 phòng doanh thu cao nhất
            TopRoomsByRevenue.Clear();
            var topRooms = _dbContext.InvoiceDetails
                .Where(d => d.Content.Contains("Tiền phòng"))
                .GroupBy(d => d.Content)
                .Select(g => new { Room = g.Key, Revenue = g.Sum(x => x.TotalPrice) })
                .OrderByDescending(x => x.Revenue)
                .Take(5)
                .ToList();
            foreach (var r in topRooms)
                TopRoomsByRevenue.Add(new TopRoomRevenue { RoomNumber = r.Room, Revenue = r.Revenue });

            // Top 5 dịch vụ sử dụng nhiều nhất
            TopServicesByUsage.Clear();
            var topServices = _dbContext.RoomServiceUsages
                .GroupBy(su => su.Service.ServiceName)
                .Select(g => new { ServiceName = g.Key, UsageCount = g.Count() })
                .OrderByDescending(x => x.UsageCount)
                .Take(5)
                .ToList();
            foreach (var s in topServices)
                TopServicesByUsage.Add(new TopServiceUsage { ServiceName = s.ServiceName, UsageCount = s.UsageCount });

            // Biểu đồ doanh thu theo ngày
            var revenueByDay = _dbContext.Invoices
                .Where(i => i.IssueDate != null && i.IssueDate.Value.Date >= FromDate && i.IssueDate.Value.Date <= ToDate)
                .GroupBy(i => i.IssueDate.Value.Date)
                .OrderBy(g => g.Key)
                .Select(g => new { Date = g.Key, Revenue = g.Sum(i => i.TotalAmount ?? 0) })
                .ToList();
            if (revenueByDay.Count == 0)
            {
                RevenueLabels = new[] { "Không có dữ liệu" };
                RevenueSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Doanh thu",
                        Values = new ChartValues<decimal> { 0 }
                    }
                };
            }
            else
            {
                RevenueLabels = revenueByDay.Select(x => x.Date.ToString("dd/MM")).ToArray();
                RevenueSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Doanh thu",
                        Values = new ChartValues<decimal>(revenueByDay.Select(x => x.Revenue))
                    }
                };
            }
            OnPropertyChanged(nameof(RevenueLabels));
            OnPropertyChanged(nameof(RevenueSeries));

            // Biểu đồ trạng thái phòng
            RoomStatusSeries = new SeriesCollection
            {
                new PieSeries { Title = "Trống", Values = new ChartValues<int> { AvailableRooms } },
                new PieSeries { Title = "Đang sử dụng", Values = new ChartValues<int> { OccupiedRooms } },
                new PieSeries { Title = "Đã đặt", Values = new ChartValues<int> { BookedRooms } },
                new PieSeries { Title = "Cần dọn dẹp", Values = new ChartValues<int> { CleaningRooms } }
            };
            OnPropertyChanged(nameof(RoomStatusSeries));

            // Biểu đồ top dịch vụ sử dụng nhiều nhất
            var topServiceList = TopServicesByUsage.ToList();
            if (topServiceList.Count == 0)
            {
                ServiceUsageLabels = new[] { "Không có dữ liệu" };
                ServiceUsageSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Số lần sử dụng",
                        Values = new ChartValues<int> { 0 }
                    }
                };
            }
            else
            {
                ServiceUsageLabels = topServiceList.Select(x => x.ServiceName).ToArray();
                ServiceUsageSeries = new SeriesCollection
                {
                    new ColumnSeries
                    {
                        Title = "Số lần sử dụng",
                        Values = new ChartValues<int>(topServiceList.Select(x => x.UsageCount))
                    }
                };
            }
            OnPropertyChanged(nameof(ServiceUsageLabels));
            OnPropertyChanged(nameof(ServiceUsageSeries));
        }
    }
} 