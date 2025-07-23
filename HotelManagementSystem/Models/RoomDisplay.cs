using Microsoft.Xaml.Behaviors.Media;
using System;
using System.Windows.Input;

namespace HotelManagementSystem.Models
{
    public class RoomDisplay
    {
        public int RoomId { get; set; } 
        public string RoomNumber { get; set; }
        public string RoomTypeName { get; set; }
        public string Status { get; set; }
        public string CleanStatus { get; set; }

        public decimal Price { get; set; }
        
        public bool IsSelected { get; set; }
        public ICommand AddRoomCommand { get; set; }
    }
} 