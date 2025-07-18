using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Booking
{
    public int BookingId { get; set; }

    public DateTime? BookingDate { get; set; }

    public DateTime? CheckIn { get; set; }

    public DateTime? CheckOut { get; set; }

    public int GuestId { get; set; }

    public int? StaffId { get; set; }

    public int? StatusId { get; set; }

    public decimal? Deposit { get; set; }

    public virtual ICollection<BookedRoom> BookedRooms { get; set; } = new List<BookedRoom>();

    public virtual Guest Guest { get; set; } = null!;

    public virtual ICollection<Invoice> Invoices { get; set; } = new List<Invoice>();

    public virtual Staff? Staff { get; set; }

    public virtual BookingStatus? Status { get; set; }
}
