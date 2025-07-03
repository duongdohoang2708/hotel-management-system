using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class ReservationRoom
{
    public int ReservationId { get; set; }

    public int RoomId { get; set; }

    public decimal RoomPrice { get; set; }

    public int? GuestCount { get; set; }

    public virtual Reservation Reservation { get; set; } = null!;

    public virtual ICollection<ReservationRoomService> ReservationRoomServices { get; set; } = new List<ReservationRoomService>();

    public virtual Room Room { get; set; } = null!;
}
