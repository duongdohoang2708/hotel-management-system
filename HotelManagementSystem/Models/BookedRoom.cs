using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class BookedRoom
{
    public int BookedRoomId { get; set; }

    public int RoomId { get; set; }

    public decimal RoomPrice { get; set; }

    public int GuestCount { get; set; }

    public int? BookingId { get; set; }

    public virtual Booking? Booking { get; set; }

    public virtual Room Room { get; set; } = null!;
}
