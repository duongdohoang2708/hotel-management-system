using HotelManagementSystem.Models;
using System;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Input;
using System.Linq; // Added for .All()

namespace HotelManagementSystem.ViewModels
{
    public class EditCustomerViewModel : ViewModelBase
    {
        private int _customerId;
        public int CustomerId
        {
            get => _customerId;
            set => SetProperty(ref _customerId, value);
        }

        private string _fullName;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string _genderDisplay;
        public string GenderDisplay
        {
            get => _genderDisplay;
            set => SetProperty(ref _genderDisplay, value);
        }

        private DateTime? _dob;
        public DateTime? Dob
        {
            get => _dob;
            set => SetProperty(ref _dob, value);
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

        public ObservableCollection<string> Genders { get; } = new ObservableCollection<string> { "Nam", "Nữ" };

        public ICommand UpdateCommand { get; }

        public event Action? RequestClose;

        public EditCustomerViewModel(Customer customer)
        {
            CustomerId = customer.CustomerId;
            FullName = customer.FullName;
            GenderDisplay = customer.Gender == "M" ? "Nam" : "Nữ";
            Dob = customer.Dob.HasValue ? customer.Dob.Value.ToDateTime(TimeOnly.MinValue) : (DateTime?)null;
            IdCardNo = customer.IdCardNo;
            Phone = customer.Phone;
            Address = customer.Address;

            UpdateCommand = new RelayCommand(_ => UpdateCustomer());
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
            if (string.IsNullOrWhiteSpace(GenderDisplay))
            {
                System.Windows.MessageBox.Show("Vui lòng chọn giới tính!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            
            // Ngày sinh là bắt buộc
            if (!Dob.HasValue)
            {
                System.Windows.MessageBox.Show("Vui lòng nhập ngày sinh!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Validate date of birth
            if (Dob.HasValue)
            {
                var today = DateTime.Today;
                var minDate = today.AddYears(-100); // Không cho phép ngày sinh quá 100 năm trước
                var maxDate = today.AddYears(-1);   // Không cho phép ngày sinh trong tương lai hoặc hôm nay
                
                if (Dob.Value > maxDate)
                {
                    System.Windows.MessageBox.Show("Ngày sinh không được là ngày trong tương lai hoặc hôm nay!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return false;
                }
                if (Dob.Value < minDate)
                {
                    System.Windows.MessageBox.Show("Ngày sinh không hợp lệ! Vui lòng nhập ngày sinh trong khoảng 100 năm trở lại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return false;
                }
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

        private void UpdateCustomer()
        {
            if (!Validate()) return;
            using (var db = new HotelManagementDbContext())
            {
                var customer = db.Customers.Find(CustomerId);
                if (customer != null)
                {
                    customer.FullName = FullName;
                    customer.Gender = GenderDisplay == "Nam" ? "M" : "F";
                    customer.Dob = Dob.HasValue ? DateOnly.FromDateTime(Dob.Value) : null;
                    // Không cho sửa IdCardNo
                    customer.Phone = Phone;
                    customer.Address = Address;
                    db.SaveChanges();
                }
            }
            System.Windows.MessageBox.Show("Cập nhật khách hàng thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 