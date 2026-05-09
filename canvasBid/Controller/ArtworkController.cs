using canvasBid.Data;
using canvasBid.Dtos;
using canvasBid.Hubs;
using canvasBid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
namespace canvasBid.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class ArtworkController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly IHubContext<BidHub> _hub;
        private readonly UserManager<User> _userManager;
        public ArtworkController(dbContext context, IHubContext<BidHub> hub, UserManager<User> userManager)
        {
            _context = context;
            _hub = hub;
            _userManager = userManager;
        }

        //create
        [Authorize(Roles = "Artist")]
        [HttpPost]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Create([FromForm] CreateArtDto dto)
        {
            var artistUser = await _userManager.FindByIdAsync(dto.artistId);

            if (artistUser == null)
                return NotFound("Artist not found");

            var roles = await _userManager.GetRolesAsync(artistUser);
            if (!roles.Contains("Artist"))
                return BadRequest("User is not an Artist");

            if (artistUser.Status != AccountStatus.Approved)
                return BadRequest("Artist account is not approved yet");

            byte[]? imageBytes = null;

            if (dto.Image != null)
            {
                using var ms = new MemoryStream();
                await dto.Image.CopyToAsync(ms);
                imageBytes = ms.ToArray();
            }

            var artwork = new Artworks
            {
                title = dto.title,
                discreption = dto.discreption,
                buyNowPrice = dto.buyNowPrice,
                intialPrice = dto.intialPrice,
                category = dto.category,
                Image = imageBytes,
                userId = dto.artistId,
                status = ArtworkStatus.Pending
            };

            _context.Artwork.Add(artwork);
            _context.SaveChanges();

            if (dto.Tags != null && dto.Tags.Any())
            {
                foreach (var tagName in dto.Tags)
                {
                    var tag = new Tags { tag = tagName, ArtworkId = artwork.artworkId };
                    _context.Tags.Add(tag);
                    _context.SaveChanges();

                    var artworkTag = new ArtworkTags
                    {
                        ArtworkId = artwork.artworkId,
                        TagId = tag.tagId
                    };
                    _context.ArtworkTags.Add(artworkTag);
                }
                _context.SaveChanges();
            }

            var result = _context.Artwork
                .Include(a => a.ArtworkTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.user)
                .FirstOrDefault(a => a.artworkId == artwork.artworkId);

            return Ok(new
            {
                result.artworkId,
                result.title,
                result.discreption,
                result.buyNowPrice,
                result.intialPrice,
                result.category,
                result.status,
                artistName = result.user.name,
                tags = result.ArtworkTags.Select(at => at.Tag.tag).ToList()
            });
        }

        //********************************************************************************************************//
        //update 
        [Authorize(Roles = "Artist")]
        [HttpPut("{id}")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Update(int id, [FromForm] UpdateArtDto dto)
        {
            var artistUser = await _userManager.FindByIdAsync(dto.artistId);

            if (artistUser == null)
                return NotFound("Artist not found");

            var roles = await _userManager.GetRolesAsync(artistUser);
            if (!roles.Contains("Artist"))
                return BadRequest("User is not an Artist");

            if (artistUser.Status != AccountStatus.Approved)
                return BadRequest("Artist account is not approved yet");

            var artwork = _context.Artwork.Find(id);

            if (artwork == null)
                return NotFound("Artwork not found");

            if (artwork.userId != dto.artistId)
                return Unauthorized("You are not the owner of this artwork");

            if (dto.title != null)
                artwork.title = dto.title;

            if (dto.discreption != null)
                artwork.discreption = dto.discreption;

            if (dto.buyNowPrice.HasValue)
                artwork.buyNowPrice = dto.buyNowPrice.Value;

            if (dto.intialPrice.HasValue)
                artwork.intialPrice = dto.intialPrice.Value;

            if (dto.category != null)
                artwork.category = dto.category;

            if (dto.Image != null)
            {
                using var ms = new MemoryStream();
                await dto.Image.CopyToAsync(ms);
                artwork.Image = ms.ToArray();
            }

            if (dto.Tags != null && dto.Tags.Any())
            {
                var oldTags = _context.ArtworkTags.Where(at => at.ArtworkId == id);
                _context.ArtworkTags.RemoveRange(oldTags);

                foreach (var tagName in dto.Tags)
                {
                    var tag = new Tags { tag = tagName, ArtworkId = artwork.artworkId };
                    _context.Tags.Add(tag);
                    _context.SaveChanges();

                    var artworkTag = new ArtworkTags
                    {
                        ArtworkId = artwork.artworkId,
                        TagId = tag.tagId
                    };
                    _context.ArtworkTags.Add(artworkTag);
                }
            }

            artwork.status = ArtworkStatus.Pending;
            _context.SaveChanges();

            var result = _context.Artwork
                .Include(a => a.ArtworkTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.user)
                .FirstOrDefault(a => a.artworkId == id);

            return Ok(new
            {
                result.artworkId,
                result.title,
                result.discreption,
                result.buyNowPrice,
                result.intialPrice,
                result.category,
                result.status,
                artistName = result.user.name,
                tags = result.ArtworkTags.Select(at => at.Tag.tag).ToList()
            });
        }

        //**************************************************************************************************************//
        //delete 
        [Authorize(Roles = "Artist")]
        [HttpDelete("{id}")]
        public IActionResult Delete(int id)
        {
            var artwork = _context.Artwork.Find(id);

            if (artwork == null) return NotFound();

            if (artwork.userId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

            _context.Artwork.Remove(artwork);
            _context.SaveChanges();

            return Ok();
        }
        //********************************************************************************************************//
        //list as artist 
        [Authorize(Roles = "Artist")]
        [HttpGet("my-artworks")]
        public IActionResult GetMyArtworks()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var artworks = _context.Artwork
                .Where(a => a.userId == userId)
                .ToList();

            return Ok(artworks);
        }
        //*******************************************************************************************************************//
        //approve
        [Authorize(Roles = "Admin")]
        [HttpPost("approve/{id}")]
        public IActionResult Approve(int id)
        {
            var artwork = _context.Artwork.Find(id);

            if (artwork == null) return NotFound();

            artwork.status = ArtworkStatus.Approved;
            _context.SaveChanges();

            return Ok("Approved");
        }
        //**************************************************************************************************************************//
        //reject
        [Authorize(Roles = "Admin")]
        [HttpPost("reject/{id}")]
        public IActionResult Reject(int id)
        {
            var artwork = _context.Artwork.Find(id);

            if (artwork == null) return NotFound();

            artwork.status = ArtworkStatus.Rejected;
            _context.SaveChanges();

            return Ok("Rejected");
        }
        //************************************************************************************************************************************//
        //listing artwork without login as buyer s
        [AllowAnonymous]
        [HttpGet("all")]
        public IActionResult GetAllArtworks()
        {
            var artworks = _context.Artwork
                .Where(a => a.status == ArtworkStatus.Approved)
                .ToList();

            return Ok(artworks);
        }
        //********************************************************************************************//
        // listing artworks
        [Authorize(Roles = "Buyer")]
        [HttpGet]
        public IActionResult GetAll()
        {
            var artworks = _context.Artwork
                .Where(a => a.status == ArtworkStatus.Approved)
                .ToList();

            return Ok(artworks);
        }
        //************************************************************************************************************************************************//
        [HttpGet("filter")]
        public IActionResult Filter(string? artist, string? category, string? tag)
        {
            var query = _context.Artwork
                .Include(a => a.ArtworkTags)
                .ThenInclude(at => at.Tag)
                .Include(a => a.user)
                .Where(a => a.status == ArtworkStatus.Approved);

            if (!string.IsNullOrEmpty(artist))
                query = query.Where(a => a.user.name.Contains(artist));

            if (!string.IsNullOrEmpty(category))
                query = query.Where(a => a.category == category);

            if (!string.IsNullOrEmpty(tag))
                query = query.Where(a =>
                    a.ArtworkTags.Any(at => at.Tag.tag.Contains(tag)));

            return Ok(query.Select(a => new {
                a.artworkId,
                a.title,
                a.discreption,
                a.buyNowPrice,
                a.intialPrice,
                a.category,
                a.status,
                artistName = a.user.name,
                tags = a.ArtworkTags.Select(at => at.Tag.tag).ToList()
            }).ToList());
        }
        //*************************************************************************************************************//

        [Authorize(Roles = "Buyer")]
        [HttpPost("watchlist/{artworkId}")]
        public IActionResult AddToWatchlist(int artworkId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var artwork = _context.Artwork.Find(artworkId);

            if (artwork == null)
                return NotFound("Artwork not found");

            if (artwork.status != ArtworkStatus.Approved)
                return BadRequest("Artwork is not approved yet");

            var exists = _context.Watchlists
                .Any(x => x.UserId == userId && x.ArtworkId == artworkId);

            if (exists)
                return BadRequest("Already added");

            var watch = new Watchlist
            {
                UserId = userId,
                ArtworkId = artworkId
            };

            _context.Watchlists.Add(watch);
            _context.SaveChanges();

            return Ok(new
            {
                message = "Added to watchlist",
                artwork = new
                {
                    artwork.Image
                }
            });
        }
        //***********************************************************************************************************//

        [Authorize(Roles = "Buyer")]
        [HttpGet("watchlist")]
        public IActionResult GetWatchlist()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var list = _context.Watchlists
            .Where(x => x.UserId == userId)
            .Include(x => x.Artwork)
            .Select(x => new watchlistResDto
          {
             ArtworkId = x.Artwork.artworkId,
             Title = x.Artwork.title,
             Price = x.Artwork.intialPrice
             })
              .ToList();

            return Ok(list);
        }
        //********************************************************************************************************************//
        [Authorize(Roles = "Artist")]
        [HttpPost("extend/{id}")]
        public async Task<IActionResult> Extend(int id, int minutes)
        {
            var artwork = _context.Artwork.Find(id);

            if (artwork == null)
                return NotFound();

            if (artwork.status != ArtworkStatus.Approved)
                return BadRequest("Cannot extend auction for non-approved artwork.");

            artwork.EndTime = artwork.EndTime.AddMinutes(minutes);

            _context.SaveChanges();
            await _hub.Clients.All
                .SendAsync("AuctionExtended", id, artwork.EndTime);

            return Ok("Extended");
        }
    }
}
 //*************************************************************************************************************//
 //get artwork by id
 [AllowAnonymous]
 [HttpGet("{id}")]
 public IActionResult GetArtworkById(int id)
 {
     var artwork = _context.Artwork
         .Where(a => a.artworkId == id) 
         .Include(a => a.ArtworkTags)
         .ThenInclude(at => at.Tag)
         .Include(a => a.user)
         .FirstOrDefault();

     if (artwork == null)
         return NotFound("Artwork not found");

     if (artwork.status != ArtworkStatus.Approved)
         return BadRequest($"Artwork is {artwork.status}");  

     return Ok(new
     {
         artwork.artworkId,
         artwork.title,
         artwork.discreption,
         artwork.buyNowPrice,
         artwork.intialPrice,
         artwork.category,
         artwork.status,
         artwork.StartTime,
         artwork.EndTime,
         artistName = artwork.user.name,
         tags = artwork.ArtworkTags.Select(at => at.Tag.tag).ToList()
     });
 }
