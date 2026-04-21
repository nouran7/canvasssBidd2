using System.Security.Cryptography;
using System.ComponentModel.DataAnnotations;
namespace canvasBid.Models
{
    public class Viewbids
    {
        
        public string UserId { get; set; }
        public User User { get; set; }

        public int BidId { get; set; }
        public Bids Bid { get; set; }
    }
}
