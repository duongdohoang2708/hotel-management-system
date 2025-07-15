using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Account
{
    public int AccountId { get; set; }

    public string Username { get; set; } = null!;

    public string Password { get; set; } = null!;

    public string Role { get; set; } = null!;

    public int StaffId { get; set; }

    public virtual Staff Staff { get; set; } = null!;
}
