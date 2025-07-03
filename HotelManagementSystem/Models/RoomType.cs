using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class RoomType
{
    public int RoomTypeId { get; set; }

    public string TypeName { get; set; } = null!;

    public decimal BasePrice { get; set; }

    public int Capacity { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<Room> Rooms { get; set; } = new List<Room>();
}
