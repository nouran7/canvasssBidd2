using Microsoft.AspNetCore.Identity;

namespace canvasBid.Models
{
    public class User: IdentityUser
    {    //identityUser bikon fiha password,id,role,email 
        public string name { get; set; }
        public string?status { get; set; }

        public List<Viewbids> bidsView { get; set; }
        public List<Viewartworks> artworkView { get; set; }
        public List<Watchlist> Watchlists { get; set; }

    }
}
