using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Bill
{
    public int BillId { get; set; }

    public int ReservationId { get; set; }

    public DateTime IssueDate { get; set; }

    public decimal RoomCharge { get; set; }

    public decimal ServiceCharge { get; set; }

    public decimal? DiscountPct { get; set; }

    public decimal? Vatpct { get; set; }

    public decimal FinalAmount { get; set; }

    public string PaymentMethod { get; set; } = null!;

    public virtual Reservation Reservation { get; set; } = null!;
}
