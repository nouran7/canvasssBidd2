namespace canvasBid.Dtos
{
    public class CreateArtDto
    {
        public string title { get; set; }
        public string discreption { get; set; }
        public decimal buyNowPrice { get; set; }
        public decimal intialPrice { get; set; }
        public string category { get; set; }
        public IFormFile? Image { get; set; }
        public List<string>? Tags { get; set; }
        public string artistId { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
    }
}
