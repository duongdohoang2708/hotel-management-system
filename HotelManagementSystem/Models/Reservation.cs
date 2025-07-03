using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Reservation
{
    public int ReservationId { get; set; }

    public int CustomerId { get; set; }

    public DateTime CheckInPlan { get; set; }

    public DateTime CheckOutPlan { get; set; }

    public decimal? Deposit { get; set; }

    public string Status { get; set; } = null!;

    public virtual ICollection<Bill> Bills { get; set; } = new List<Bill>();

    public virtual Customer Customer { get; set; } = null!;

    public virtual ICollection<ReservationRoom> ReservationRooms { get; set; } = new List<ReservationRoom>();
}
