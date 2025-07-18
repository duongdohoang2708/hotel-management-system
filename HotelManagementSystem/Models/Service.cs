using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Service
{
    public int ServiceId { get; set; }

    public string? ServiceName { get; set; }

    public decimal? UnitPrice { get; set; }

    public int CategoryId { get; set; }

    public virtual ServiceCategory Category { get; set; } = null!;

    public virtual ICollection<RoomServiceUsage> RoomServiceUsages { get; set; } = new List<RoomServiceUsage>();
}
