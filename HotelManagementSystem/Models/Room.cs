using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string RoomNumber { get; set; } = null!;

    public int RoomTypeId { get; set; }

    public int? Floor { get; set; }

    public string Status { get; set; } = null!;

    public string CleanStatus { get; set; } = null!;

    public virtual ICollection<BookedRoom> BookedRooms { get; set; } = new List<BookedRoom>();

    public virtual ICollection<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();

    public virtual RoomType RoomType { get; set; } = null!;
}
