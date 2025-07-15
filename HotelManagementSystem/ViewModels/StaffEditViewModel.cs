using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class StaffEditViewModel : ViewModelBase
    {
        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string? _role;
        public string? Role
        {
            get => _role;
            set => SetProperty(ref _role, value);
        }

        private string? _phone;
        public string? Phone
        {
            get => _phone;
            set => SetProperty(ref _phone, value);
        }

        private string? _email;
        public string? Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        private DateOnly? _dateOfBirth;
        public DateOnly? DateOfBirth
        {
            get => _dateOfBirth;
            set => SetProperty(ref _dateOfBirth, value);
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Staff>? EmployeeSaved;

        private readonly Staff? _editingStaff;
        private readonly bool _isEditMode;

        public StaffEditViewModel(Staff? staff = null)
        {
            if (staff != null)
            {
                _isEditMode = true;
                _editingStaff = staff;
                FullName = staff.FullName;
                Role = staff.Role;
                Phone = staff.Phone;
                Email = staff.Email;
                DateOfBirth = staff.DateOfBirth;
            }
            SaveCommand = new RelayCommand(_ => Save());
            CancelCommand = new RelayCommand(_ => RequestClose?.Invoke());
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
            // Họ tên
            if (string.IsNullOrWhiteSpace(FullName))
            {
                System.Windows.MessageBox.Show("Họ tên không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (ContainsSpecialChar(FullName))
            {
                System.Windows.MessageBox.Show("Họ tên không được chứa ký tự đặc biệt!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Ngày sinh
            if (DateOfBirth == null)
            {
                System.Windows.MessageBox.Show("Vui lòng nhập ngày sinh!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            var today = DateOnly.FromDateTime(DateTime.Today);
            var minDate = today.AddYears(-100);
            var maxDate = today.AddYears(-1);
            if (DateOfBirth > maxDate)
            {
                System.Windows.MessageBox.Show("Ngày sinh không được là ngày trong tương lai hoặc hôm nay!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            if (DateOfBirth < minDate)
            {
                System.Windows.MessageBox.Show("Ngày sinh không hợp lệ! Vui lòng nhập ngày sinh trong khoảng 100 năm trở lại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // SĐT
            if (string.IsNullOrWhiteSpace(Phone) || !Regex.IsMatch(Phone, @"^\d{10}$"))
            {
                System.Windows.MessageBox.Show("Số điện thoại phải đủ 10 số!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Email
            if (!string.IsNullOrWhiteSpace(Email))
            {
                if (!Regex.IsMatch(Email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$"))
                {
                    System.Windows.MessageBox.Show("Email không hợp lệ!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return false;
                }
                using (var db = new HotelManagementDbContext())
                {
                    var existing = db.Staff.FirstOrDefault(s => s.Email == Email);
                    if (existing != null && (!_isEditMode || existing.StaffId != _editingStaff?.StaffId))
                    {
                        System.Windows.MessageBox.Show($"Email '{Email}' đã tồn tại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return false;
                    }
                }
            }
            return true;
        }

        private void Save()
        {
            if (!Validate()) return;
            using (var db = new HotelManagementDbContext())
            {
                Staff staff;
                if (_isEditMode && _editingStaff != null)
                {
                    staff = db.Staff.FirstOrDefault(s => s.StaffId == _editingStaff.StaffId) ?? _editingStaff;
                    staff.FullName = FullName;
                    staff.Role = Role;
                    staff.Phone = Phone;
                    staff.Email = Email;
                    staff.DateOfBirth = DateOfBirth;
                }
                else
                {
                    staff = new Staff
                    {
                        FullName = FullName,
                        Role = Role,
                        Phone = Phone,
                        Email = Email,
                        DateOfBirth = DateOfBirth
                    };
                    db.Staff.Add(staff);
                }
                db.SaveChanges();
                EmployeeSaved?.Invoke(staff);
            }
            System.Windows.MessageBox.Show("Lưu nhân viên thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 