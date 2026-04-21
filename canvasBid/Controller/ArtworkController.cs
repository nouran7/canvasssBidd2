using canvasBid.Data;
using canvasBid.Dtos;
using canvasBid.Hubs;
using canvasBid.Models;
using Microsoft.AspNetCore.Authorization;
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
        public ArtworkController(dbContext context, IHubContext<BidHub> hub)
        {
            _context = context;
            _hub = hub;
        }

       //create

        [Authorize(Roles = "Artist")]
        [HttpPost]
        public IActionResult Create(CreateArtDto dto)
        {
            var artwork = new Artworks
            {
                title = dto.title,
                discreption = dto.discreption,
                buyNowPrice = dto.buyNowPrice,
                intialPrice = dto.intialPrice,
                category = dto.category,
                Image = dto.Image,

                userId = User.FindFirstValue(ClaimTypes.NameIdentifier),
                status = ArtworkStatus.Pending
            };

            _context.Artwork.Add(artwork);
            _context.SaveChanges();

            return Ok(artwork);
        }

        //********************************************************************************************************//
        //update 
        [Authorize(Roles = "Artist")]
        [HttpPut("{id}")]
        public IActionResult Update(int id, UpdateArtDto dto)
        {
            var artwork = _context.Artwork.Find(id);

            if (artwork == null)
                return NotFound();

            if (artwork.userId != User.FindFirstValue(ClaimTypes.NameIdentifier))
                return Unauthorized();

           
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
                artwork.Image = dto.Image;

            artwork.status = ArtworkStatus.Pending;

            _context.SaveChanges();

            return Ok(artwork);
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
        public IActionResult Filter(string? artist, string? category, int? tagId)
        {
            var query = _context.Artwork
                .Include(a => a.ArtworkTags)
                .Where(a => a.status == ArtworkStatus.Approved);

            if (!string.IsNullOrEmpty(artist))
                query = query.Where(a => a.userId == artist);

            if (!string.IsNullOrEmpty(category))
                query = query.Where(a => a.category == category);

            
            if (tagId.HasValue)
                query = query.Where(a =>
                    a.ArtworkTags.Any(at => at.TagId == tagId.Value));

            return Ok(query.ToList());
        }
        //*************************************************************************************************************//

        [Authorize(Roles = "Buyer")]
        [HttpPost("watchlist/{artworkId}")]
        public IActionResult AddToWatchlist(int artworkId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

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

            return Ok("Added to watchlist");
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

            artwork.EndTime = artwork.EndTime.AddMinutes(minutes);

            _context.SaveChanges();
            await _hub.Clients.All
       .SendAsync("AuctionExtended", id, artwork.EndTime);

            return Ok("Extended");
        }
    }
}
