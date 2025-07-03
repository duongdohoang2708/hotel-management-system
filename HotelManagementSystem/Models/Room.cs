using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public int RoomTypeId { get; set; }

    public int? Floor { get; set; }

    public string Status { get; set; } = null!;

    public string CleanStatus { get; set; } = null!;

    public virtual ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();

    public virtual RoomType RoomType { get; set; } = null!;

    public virtual ICollection<Amenity> Amenities { get; set; } = new List<Amenity>();
}
