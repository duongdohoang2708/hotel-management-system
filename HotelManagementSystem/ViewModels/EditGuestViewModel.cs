using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Linq; // Added for .All()

namespace HotelManagementSystem.ViewModels
{
    public class EditGuestViewModel : ViewModelBase
    {
        private int _guestId;
        public int GuestId
        {
            get => _guestId;
            set => SetProperty(ref _guestId, value);
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string _idCardNo;
        public string IdCardNo
        {
            get => _idCardNo;
            set => SetProperty(ref _idCardNo, value);
        }

        private string _phone;
        public string Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string _address;
        public string Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        public ICommand UpdateCommand { get; }

        public event Action? RequestClose;

        public EditGuestViewModel(Guest guest)
        {
            GuestId = guest.GuestId;
            FullName = guest.FullName;
            IdCardNo = guest.IdCardNo;
            Phone = guest.Phone;
            Address = guest.Address;
            UpdateCommand = new RelayCommand(_ => UpdateGuest());
        }

        private bool ContainsSpecialChar(string input)
        {
            // Cho phép chữ cái, số, khoảng trắng, dấu tiếng Việt, dấu gạch ngang, dấu chấm, dấu phẩy
            // Không cho phép ký tự đặc biệt như !@#$%^&*()_+=[]{}|;:'"<>,/?~
            if (string.IsNullOrEmpty(input)) return false;
            foreach (char c in input)
            {
                if (!(char.IsLetterOrDigit(c) || char.IsWhiteSpace(c) ||
                      "àáảãạâầấẩẫậăằắẳẵặèéẻẽẹêềếểễệìíỉĩịòóỏõọôồốổỗộơờớởỡợùúủũụưừứửữựỳýỷỹỵđÀÁẢÃẠÂẦẤẨẪẬĂẰẮẲẴẶÈÉẺẼẸÊỀẾỂỄỆÌÍỈĨỊÒÓỎÕỌÔỒỐỔỖỘƠỜỚỞỠỢÙÚỦŨỤƯỪỨỬỮỰỲÝỶỸỴĐ-.,".Contains(c)))
                {
                    return true;
                }
            }
            return false;
        }

        private bool Validate()
        {
            if (string.IsNullOrWhiteSpace(FullName))
            {
                System.Windows.MessageBox.Show("Họ và tên không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (ContainsSpecialChar(FullName))
            {
                System.Windows.MessageBox.Show("Họ và tên không được chứa ký tự đặc biệt!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Phone) || Phone.Length != 10 || !Phone.All(char.IsDigit))
            {
                System.Windows.MessageBox.Show("Số điện thoại phải đủ 10 số!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (string.IsNullOrWhiteSpace(Address))
            {
                System.Windows.MessageBox.Show("Địa chỉ không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (ContainsSpecialChar(Address))
            {
                System.Windows.MessageBox.Show("Địa chỉ không được chứa ký tự đặc biệt!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            return true;
        }

        private void UpdateGuest()
        {
            if (!Validate()) return;
            using (var db = new HotelManagementDbContext())
            {
                var guest = db.Guests.Find(GuestId);
                if (guest != null)
                {
                    guest.FullName = FullName;
                    // Không cho sửa IdCardNo
                    guest.Phone = Phone;
                    guest.Address = Address;
                    db.SaveChanges();
                }
            }
            System.Windows.MessageBox.Show("Cập nhật khách thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 