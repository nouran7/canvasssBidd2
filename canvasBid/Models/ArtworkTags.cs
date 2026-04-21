using Azure;

namespace canvasBid.Models
{
    public class ArtworkTags
    {
        public int ArtworkId { get; set; }
        public Artworks Artwork { get; set; }

        public int TagId { get; set; }
        public Tags Tag { get; set; }
    }
}
