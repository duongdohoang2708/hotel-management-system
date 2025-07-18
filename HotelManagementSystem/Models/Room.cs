using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string? RoomNumber { get; set; }

    public string? Status { get; set; }

    public string? CleanStatus { get; set; }

    public int RoomTypeId { get; set; }

    public virtual ICollection<BookedRoom> BookedRooms { get; set; } = new List<BookedRoom>();

    public virtual ICollection<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();

    public virtual RoomType RoomType { get; set; } = null!;
}
