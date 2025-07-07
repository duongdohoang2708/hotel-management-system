using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class RoomAmenity
{
    public int RoomId { get; set; }

    public int AmenityId { get; set; }

    public int Quantity { get; set; }

    public virtual Amenity Amenity { get; set; } = null!;

    public virtual Room Room { get; set; } = null!;
}
