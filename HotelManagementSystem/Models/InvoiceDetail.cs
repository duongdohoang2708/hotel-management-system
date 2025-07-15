using System;
using System.Collections.Generic;

namespace HotelManagementSystem.Models;

public partial class InvoiceDetail
{
    public int InvoiceItemId { get; set; }

    public int InvoiceId { get; set; }

    public string? ItemType { get; set; }

    public int Quantity { get; set; }

    public decimal UnitPrice { get; set; }

    public decimal TotalPrice { get; set; }

    public virtual Invoice Invoice { get; set; } = null!;
}
