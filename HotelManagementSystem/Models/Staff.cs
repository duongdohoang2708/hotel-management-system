using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string FullName { get; set; } = null!;

    public string? Role { get; set; }

    public string? Phone { get; set; }

    public string? Email { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
