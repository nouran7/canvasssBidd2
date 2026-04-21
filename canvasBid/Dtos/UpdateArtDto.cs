namespace canvasBid.Dtos
{
    public class UpdateArtDto
    {
        public string? title { get; set; }
        public string? discreption { get; set; }
        public decimal? buyNowPrice { get; set; }
        public decimal? intialPrice { get; set; }
        public string? category { get; set; }
        public byte[]? Image { get; set; }
    }
}
