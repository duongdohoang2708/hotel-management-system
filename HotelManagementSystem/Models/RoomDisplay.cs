using System;

namespace HotelManagementSystem.Models
{
    public class RoomDisplay
    {
        public int RoomId { get; set; } 
        public string RoomNumber { get; set; }
        public string RoomType { get; set; }
        public decimal Price { get; set; }
        public int Capacity { get; set; }
        public int Floor { get; set; }
        public bool IsSelected { get; set; }
    }
} 