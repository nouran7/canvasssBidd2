using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
namespace canvasBid.Models
{
    public class Bids
    {
        [Key]
        public int bid_id { get; set; }
        public decimal amount { get; set; }
        public decimal price { get; set; }
        public DateTime timeStamp { get; set; }
        public string UserId { get; set; }
        public User User { get; set; }

        public List<Viewbids> viewUsers { get; set; }
        public int ArtworkId { get; set; }
        public Artworks Artwork { get; set; }

    }
}
