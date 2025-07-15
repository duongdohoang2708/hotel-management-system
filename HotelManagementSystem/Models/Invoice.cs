using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class Invoice
{
    public int InvoiceId { get; set; }

    public int BookingId { get; set; }

    public DateTime IssueDate { get; set; }

    public decimal? Vat { get; set; }

    public decimal TotalAmount { get; set; }

    public int StaffId { get; set; }

    public virtual Booking Booking { get; set; } = null!;

    public virtual ICollection<InvoiceDetail> InvoiceDetails { get; set; } = new List<InvoiceDetail>();

    public virtual Staff Staff { get; set; } = null!;
}
