using System;
using System.Windows.Input;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class StaffAddEditViewModel : ViewModelBase
    {
        private int _staffId;
        public int StaffId
        {
            get => _staffId;
            set => SetProperty(ref _staffId, value);
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string _email;
        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string _position;
        public string Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
        }

        public bool IsEditMode { get; set; }
        public string WindowTitle => IsEditMode ? "Sửa nhân viên" : "Thêm nhân viên";

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Staff>? StaffSaved;

        public StaffAddEditViewModel(bool isEditMode = false, Staff? staff = null)
        {
            IsEditMode = isEditMode;
            if (isEditMode && staff != null)
            {
                StaffId = staff.StaffId;
                FullName = staff.FullName;
                Email = staff.Email;
                Phone = staff.Phone;
                Position = staff.Position;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
        }

        private void Save()
        {
            using (var db = new HotelManagementDbContext())
            {
                if (IsEditMode)
                {
                    var s = db.Staff.Find(StaffId);
                    if (s != null)
                    {
                        s.FullName = FullName;
                        s.Email = Email;
                        s.Phone = Phone;
                        s.Position = Position;
                        db.SaveChanges();
                        StaffSaved?.Invoke(s);
                    }
                }
                else
                {
                    var s = new Staff
                    {
                        FullName = FullName,
                        Email = Email,
                        Phone = Phone,
                        Position = Position
                    };
                    db.Staff.Add(s);
                    db.SaveChanges();
                    StaffSaved?.Invoke(s);
                }
            }
            RequestClose?.Invoke();
        }
    }
} 