using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Staff
{
    public int StaffId { get; set; }

    public string? FullName { get; set; }

    public string? Email { get; set; }

    public string? Phone { get; set; }

    public string? Position { get; set; }

    public virtual ICollection<Account> Accounts { get; set; } = new List<Account>();

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();
}
