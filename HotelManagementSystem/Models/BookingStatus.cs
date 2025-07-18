using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class BookingStatus
{
    public int StatusId { get; set; }

    public string StatusName { get; set; } = null!;

    public virtual ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}
