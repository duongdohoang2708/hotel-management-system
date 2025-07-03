using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class ReservationRoomService
{
    public int ServiceLineId { get; set; }

    public int ReservationId { get; set; }

    public int RoomId { get; set; }

    public int ServiceId { get; set; }

    public int? Qty { get; set; }

    public decimal UnitPrice { get; set; }

    public virtual ReservationRoom ReservationRoom { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
