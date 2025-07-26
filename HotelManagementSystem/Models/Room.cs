using System;
using System.Collections.Generic;
using System.Linq;

namespace HotelManagementSystem.Models;

public partial class Room
{
    public int RoomId { get; set; }

    public string? RoomNumber { get; set; }

    public string? CleanStatus { get; set; }

    public int RoomTypeId { get; set; }

    public virtual ICollection<BookedRoom> BookedRooms { get; set; } = new List<BookedRoom>();

    public virtual ICollection<RoomFacility> RoomFacilities { get; set; } = new List<RoomFacility>();

    public virtual RoomType RoomType { get; set; } = null!;

    public string GetStatusByNow(IEnumerable<Booking> allBookings)
    {
        var now = DateTime.Now;
        var bookings = allBookings
            .Where(b => b.BookedRooms.Any(br => br.RoomId == this.RoomId))
            .ToList();

        var inUse = bookings.FirstOrDefault(b =>
            b.CheckIn <= now && b.CheckOut > now && b.StatusId == 2 // 2 = Đã nhận phòng
        );
        if (inUse != null)
            return "Có khách";

        var booked = bookings.FirstOrDefault(b =>
            b.CheckIn > now && b.StatusId == 1 // 1 = Đã đặt
        );
        if (booked != null)
            return "Đã đặt";

        return "Trống";
    }

    public string GetStatusByDuration(DateTime checkInKey, DateTime checkOutKey, IEnumerable<Booking> allBookings)
    {
        var bookings = allBookings
            .Where(b => b.BookedRooms.Any(br => br.RoomId == this.RoomId))
            .ToList();

        // Kiểm tra xem có booking nào trùng với khoảng thời gian này không
        // Hai khoảng thời gian trùng nhau khi: existing_checkin < new_checkout && existing_checkout > new_checkin
        var conflictingBooking = bookings.FirstOrDefault(b =>
           b.CheckIn < checkOutKey && 
           b.CheckOut > checkInKey && 
            (b.StatusId == 1 || b.StatusId == 2) // 1 = Đã đặt, 2 = Đã nhận phòng
        );

        if (conflictingBooking != null)
            return "Đã đặt";

        return "Trống";
    }
}
