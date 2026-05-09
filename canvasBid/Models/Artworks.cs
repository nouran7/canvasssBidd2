using System.ComponentModel.DataAnnotations;
using System.Security.Cryptography;

namespace canvasBid.Models
{
    public class Artworks
    {
        [Key]
        public int artworkId { get; set; }
        public string title { get; set; }
        public string discreption { get; set; }
        public decimal buyNowPrice { get; set; }
        public string category { get; set; }
        public byte[]? Image { get; set; }
        public decimal intialPrice { get; set; }
        public ArtworkStatus status { get; set; }

        public string userId { get; set; }
        public User user { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public List<Viewartworks> userView { get; set; }
        public List<ArtworkTags> ArtworkTags { get; set; }
        public List<Bids> Bids { get; set; }
        public List<Watchlist> Watchlists { get; set; }
    }
}
