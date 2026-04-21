namespace canvasBid.Models
{
    public class Watchlist
    {
        public string UserId { get; set; }
        public User User { get; set; }

        public int ArtworkId { get; set; }
        public Artworks Artwork { get; set; }
    }
}
