using System;
using System.Windows.Input;
using System.ComponentModel;

namespace HotelManagementSystem.Models
{
    public class RoomDisplayForRoomManagement : INotifyPropertyChanged
    {
        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        private int _roomId;
        public int RoomId
        {
            get => _roomId;
            set { if (_roomId != value) { _roomId = value; OnPropertyChanged(nameof(RoomId)); } }
        }

        private string _roomNumber;
        public string RoomNumber
        {
            get => _roomNumber;
            set { if (_roomNumber != value) { _roomNumber = value; OnPropertyChanged(nameof(RoomNumber)); } }
        }

        private string _roomType;
        public string RoomType
        {
            get => _roomType;
            set { if (_roomType != value) { _roomType = value; OnPropertyChanged(nameof(RoomType)); } }
        }

        private decimal _price;
        public decimal Price
        {
            get => _price;
            set { if (_price != value) { _price = value; OnPropertyChanged(nameof(Price)); } }
        }

        private bool _isSelected;
        public bool IsSelected
        {
            get => _isSelected;
            set { if (_isSelected != value) { _isSelected = value; OnPropertyChanged(nameof(IsSelected)); } }
        }

        public ICommand AddRoomCommand { get; set; }
    }
} 