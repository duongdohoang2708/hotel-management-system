using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public int CategoryId { get; set; }

    public string ServiceName { get; set; } = null!;

    public decimal UnitPrice { get; set; }

    public virtual ServiceCategory Category { get; set; } = null!;

    public virtual ICollection<ReservationRoomService> ReservationRoomServices { get; set; } = new List<ReservationRoomService>();
}
