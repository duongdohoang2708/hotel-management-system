using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows.Input;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.ViewModels
{
    public class EmployeeEditViewModel : ViewModelBase
    {
        private string _fullName = string.Empty;
        public string FullName
        {
            get => _fullName;
            set => SetProperty(ref _fullName, value);
        }

        private string? _position;
        public string? Position
        {
            get => _position;
            set => SetProperty(ref _position, value);
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

        private string? _gender;
        public string? Gender
        {
            get => _gender;
            set => SetProperty(ref _gender, value);
        }

        private string? _address;
        public string? Address
        {
            get => _address;
            set => SetProperty(ref _address, value);
        }

        private string? _citizenId;
        public string? CitizenId
        {
            get => _citizenId;
            set => SetProperty(ref _citizenId, value);
        }

        private decimal? _salary;
        public decimal? Salary
        {
            get => _salary;
            set => SetProperty(ref _salary, value);
        }

        public ObservableCollection<string> Genders { get; } = new ObservableCollection<string> { "Nam", "Nữ" };

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public event Action? RequestClose;
        public event Action<Employee>? EmployeeSaved;

        private readonly Employee? _editingEmployee;
        private readonly bool _isEditMode;

        public EmployeeEditViewModel(Employee? employee = null)
        {
            if (employee != null)
            {
                _isEditMode = true;
                _editingEmployee = employee;
                FullName = employee.FullName;
                Position = employee.Position;
                Phone = employee.Phone;
                Email = employee.Email;
                DateOfBirth = employee.DateOfBirth;
                Gender = employee.Gender == "M" ? "Nam" : "Nữ"; ;
                Address = employee.Address;
                CitizenId = employee.CitizenId;
                Salary = employee.Salary;
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
            // Giới tính
            if (string.IsNullOrWhiteSpace(Gender) || !(Gender == "Nam" || Gender == "Nữ"))
            {
                System.Windows.MessageBox.Show("Vui lòng chọn giới tính!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                    var existing = db.Employees.FirstOrDefault(e => e.Email == Email);
                    if (existing != null && (!_isEditMode || existing.EmployeeId != _editingEmployee?.EmployeeId))
                    {
                        System.Windows.MessageBox.Show($"Email '{Email}' đã tồn tại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                        return false;
                    }
                }
            }
            // Lương
            if (Salary != null && Salary < 0)
            {
                System.Windows.MessageBox.Show("Lương phải >= 0!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // Địa chỉ
            if (!string.IsNullOrWhiteSpace(Address) && ContainsSpecialChar(Address))
            {
                System.Windows.MessageBox.Show("Địa chỉ không được chứa ký tự đặc biệt!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                return false;
            }
            // CCCD
            if (!_isEditMode)
            {
                if (string.IsNullOrWhiteSpace(CitizenId))
                {
                    System.Windows.MessageBox.Show("CCCD không được để trống!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
                    return false;
                }
                using (var db = new HotelManagementDbContext())
                {
                    var existing = db.Employees.FirstOrDefault(e => e.CitizenId == CitizenId);
                    if (existing != null)
                    {
                        System.Windows.MessageBox.Show($"CCCD '{CitizenId}' đã tồn tại!", "Lỗi", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
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
                Employee employee;
                if (_isEditMode && _editingEmployee != null)
                {
                    employee = db.Employees.FirstOrDefault(e => e.EmployeeId == _editingEmployee.EmployeeId) ?? _editingEmployee;
                    employee.FullName = FullName;
                    employee.Position = Position;
                    employee.Phone = Phone;
                    employee.Email = Email;
                    employee.DateOfBirth = DateOfBirth;
                    employee.Gender = Gender;
                    employee.Address = Address;
                    employee.Salary = Salary;
                    // Không cho sửa CCCD khi edit
                }
                else
                {
                    employee = new Employee
                    {
                        FullName = FullName,
                        Position = Position,
                        Phone = Phone,
                        Email = Email,
                        DateOfBirth = DateOfBirth,
                        Gender = Gender,
                        Address = Address,
                        CitizenId = CitizenId,
                        Salary = Salary
                    };
                    db.Employees.Add(employee);
                }
                db.SaveChanges();
                EmployeeSaved?.Invoke(employee);
            }
            System.Windows.MessageBox.Show("Lưu nhân viên thành công!", "Thông báo", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
            RequestClose?.Invoke();
        }
    }
} 