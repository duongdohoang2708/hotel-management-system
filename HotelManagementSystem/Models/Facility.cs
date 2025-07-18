using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Facility
{
    public int FacilityId { get; set; }

    public string? FacilityName { get; set; }

    public string? Description { get; set; }

    public virtual ICollection<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();
}
