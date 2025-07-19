namespace HotelManagementSystem.Models
{
    public class RoomFacilityDisplay
    {
        public int RoomFacilityId { get; set; }
        public int RoomId { get; set; }
        public int FacilityId { get; set; }
        public string FacilityName { get; set; }
        public string Description { get; set; }
        public int? Quantity { get; set; }
    }
} 