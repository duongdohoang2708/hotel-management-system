using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Windows.Input;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagementSystem.ViewModels
{
    public class BookingAddEditViewModel : ViewModelBase, INotifyDataErrorInfo
    {
        public event Action<Booking> BookingSaved;

        private Booking _booking;
        public Booking Booking
        {
            get => _booking;
            set
            {
                if (SetProperty(ref _booking, value))
                {
                    ValidateAll();
                }
            }
        }

        public ObservableCollection<Guest> Guests { get; set; } = new ObservableCollection<Guest>();
        public ObservableCollection<BookingStatus> Statuses { get; set; } = new ObservableCollection<BookingStatus>();

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public BookingAddEditViewModel(Booking booking, ObservableCollection<Guest> guests, ObservableCollection<BookingStatus> statuses)
        {
            Booking = booking;
            foreach (var g in guests) Guests.Add(g);
            foreach (var s in statuses) Statuses.Add(s);
            SaveCommand = new RelayCommand(_ => Save(), _ => !HasErrors);
            CancelCommand = new RelayCommand(_ => Cancel());
        }

        // Validation logic
        private readonly Dictionary<string, List<string>> _errors = new();
        public bool HasErrors => _errors.Any();
        public event EventHandler<DataErrorsChangedEventArgs> ErrorsChanged;
        public System.Collections.IEnumerable GetErrors(string propertyName)
        {
            if (string.IsNullOrEmpty(propertyName)) return _errors.SelectMany(e => e.Value);
            if (_errors.TryGetValue(propertyName, out var errors)) return errors;
            return null;
        }
        private void AddError(string propertyName, string error)
        {
            if (!_errors.ContainsKey(propertyName)) _errors[propertyName] = new List<string>();
            if (!_errors[propertyName].Contains(error))
            {
                _errors[propertyName].Add(error);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
        private void ClearErrors(string propertyName)
        {
            if (_errors.ContainsKey(propertyName))
            {
                _errors.Remove(propertyName);
                ErrorsChanged?.Invoke(this, new DataErrorsChangedEventArgs(propertyName));
            }
        }
        private void ValidateAll()
        {
            ValidateGuest();
            ValidateCheckIn();
            ValidateCheckOut();
            ValidateDeposit();
            
        }
        private void ValidateGuest()
        {
            ClearErrors(nameof(Booking.Guest));
            if (Booking.Guest == null)
                AddError(nameof(Booking.Guest), "Vui lòng chọn khách hàng.");
        }
        private void ValidateCheckIn()
        {
            ClearErrors(nameof(Booking.CheckIn));
            if (Booking.CheckIn == null)
                AddError(nameof(Booking.CheckIn), "Vui lòng chọn ngày nhận phòng.");
            else if (Booking.CheckIn.Value.Date < DateTime.Today)
                AddError(nameof(Booking.CheckIn), "Ngày nhận không được nhỏ hơn hôm nay.");
        }
        private void ValidateCheckOut()
        {
            ClearErrors(nameof(Booking.CheckOut));
            if (Booking.CheckOut == null)
                AddError(nameof(Booking.CheckOut), "Vui lòng chọn ngày trả phòng.");
            else if (Booking.CheckIn != null && Booking.CheckOut <= Booking.CheckIn)
                AddError(nameof(Booking.CheckOut), "Ngày trả phải sau ngày nhận.");
        }
        private void ValidateDeposit()
        {
            ClearErrors(nameof(Booking.Deposit));
            if (Booking.Deposit == null)
                AddError(nameof(Booking.Deposit), "Vui lòng nhập tiền cọc.");
            else if (Booking.Deposit < 0)
                AddError(nameof(Booking.Deposit), "Tiền cọc không được âm.");
        }
        
        private void Save()
        {
            ValidateAll();
            if (!HasErrors)
                BookingSaved?.Invoke(Booking);
        }
        private void Cancel()
        {
            BookingSaved?.Invoke(null);
        }
    }
} 