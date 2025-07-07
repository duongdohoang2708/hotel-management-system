using System.Security.Cryptography;
using System.Text;
using System.Linq;
using HotelManagementSystem.Models;

namespace HotelManagementSystem.Services
{
    public class AccountService
    {
        private readonly HotelManagementDbContext _context;

        public AccountService(HotelManagementDbContext context)
        {
            _context = context;
        }

        public Account? Authenticate(string username, string password)
        {
            var hash = HashPassword(password);
            return _context.Accounts.FirstOrDefault(a => a.Username == username && a.PasswordHash == hash);
        }

        public bool ChangePassword(string username, string newPassword)
        {
            var account = _context.Accounts.FirstOrDefault(a => a.Username == username);
            if(account == null) return false;
            account.PasswordHash = HashPassword(newPassword);
            _context.SaveChanges();
            return true;
        }

        public static string HashPassword(string password)
        {
            using (var sha256 = SHA256.Create())
            {
                var bytes = Encoding.Unicode.GetBytes(password); // SQL dùng NVARCHAR => Encoding.Unicode
                var hashBytes = sha256.ComputeHash(bytes);
                var sb = new StringBuilder();
                foreach (var b in hashBytes)
                    sb.Append(b.ToString("X2")); // SQL dùng kiểu HEX in hoa
                return sb.ToString();
            }
        }
    }
} 