using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class RoomServiceUsage
{
    public int UsageId { get; set; }

    public int BookedRoomId { get; set; }

    public int ServiceId { get; set; }

    public DateTime? UsageTime { get; set; }

    public decimal? UnitPrice { get; set; }

    public int? Quantity { get; set; }

    public virtual BookedRoom BookedRoom { get; set; } = null!;

    public virtual Service Service { get; set; } = null!;
}
