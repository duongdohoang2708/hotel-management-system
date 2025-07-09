using System;

namespace HotelManagementSystem.Models
{
    public class RoomStatusItem
    {
        public int RoomId { get; set; }
        public string RoomNumber { get; set; } = string.Empty;
        public string RoomType { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string CleanStatus { get; set; } = string.Empty;
        public string? GuestName { get; set; }
        public DateTime? CheckInPlan { get; set; }
        public DateTime? CheckOutPlan { get; set; }
        public string? StayDuration { get; set; } // Ví dụ: "2 ngày", "3 giờ"
        public string CardBackground { get; set; } = "#FFF";
        public string CleanStatusIcon { get; set; } = "Check";
        public string CleanStatusIconColor { get; set; } = "#388E3C";
        public string CardForeground { get; set; } = "#222";
    }
} 