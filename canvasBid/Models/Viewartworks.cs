using System.Globalization;
using System.ComponentModel.DataAnnotations;
namespace canvasBid.Models
{
    public class Viewartworks
    {
        
        public String userId { get; set; }
        public User user { get; set; }
        public int artworkId { get; set; }
        public Artworks artwork { get; set; }
    }
}
