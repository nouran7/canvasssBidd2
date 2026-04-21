namespace canvasBid.Dtos
{
    public class bidResDto
    {
        public int BidId { get; set; }
        public decimal Amount { get; set; }
        public DateTime TimeStamp { get; set; }
        public int ArtworkId { get; set; }
        public string UserId { get; set; }
    }
}
