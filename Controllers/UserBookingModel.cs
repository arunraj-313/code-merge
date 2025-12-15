namespace StayEasePG.Controllers
{
    internal class UserBookingModel
    {
        public int BookingID { get; set; }
        public string PGName { get; set; }
        public string City { get; set; }
        public string PGAddress { get; set; }
        public string RoomType { get; set; }
        public string PaymentStatus { get; set; }
        public string BookingStatus { get; set; }
    }
}