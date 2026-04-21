using System.ComponentModel.DataAnnotations;

namespace canvasBid.Models
{
    public class Tags
    {
        [Key]
        public int tagId { get; set; }
        public String tag { get; set; }
        public int ArtworkId { get; set; }
        public Artworks artwork { get; set; }
        public List<ArtworkTags> ArtworkTags { get; set; }
    }
}
