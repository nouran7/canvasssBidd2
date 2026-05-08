using canvasBid.Data;
using canvasBid.Dtos;
using canvasBid.Hubs;
using canvasBid.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Security.Cryptography;
using Microsoft.AspNetCore.SignalR;
namespace canvasBid.Controller
{
    [Route("api/[controller]")]
    [ApiController]
    public class BidController : ControllerBase
    {
        private readonly dbContext _context;
        private readonly IHubContext<BidHub> _hub;
        public BidController(dbContext context, IHubContext<BidHub> hub)
        {
            _context = context;
            _hub = hub;
        }
        //place bid on artwork
        [Authorize(Roles = "Buyer")]
        [HttpPost]
        public async Task<IActionResult> PlaceBid(bidReqDto dto)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var artwork = _context.Artwork
                .Include(a => a.Bids)
                .FirstOrDefault(a => a.artworkId == dto.ArtworkId);

            if (artwork == null)
                return NotFound();

            
            if (artwork.status != ArtworkStatus.Approved)
                return BadRequest("Cannot place bid on non-approved artwork.");

            var lastBid = artwork.Bids
                .OrderByDescending(b => b.amount)
                .FirstOrDefault();

            decimal minAllowed = lastBid != null
                ? lastBid.amount + 10
                : artwork.intialPrice;

            if (dto.Amount < minAllowed)
                return BadRequest($"Bid must be at least {minAllowed}");

            var bid = new Bids
            {
                amount = dto.Amount,
                ArtworkId = dto.ArtworkId,
                UserId = userId,
                timeStamp = DateTime.Now
            };

            _context.Bid.Add(bid);
            await _context.SaveChangesAsync();

            await _hub.Clients.All
                .SendAsync("ReceiveBid", dto.ArtworkId, dto.Amount);

            var result = new bidResDto
            {
                BidId = bid.bid_id,
                Amount = bid.amount,
                TimeStamp = bid.timeStamp,
                ArtworkId = bid.ArtworkId,
                UserId = bid.UserId
            };

            return Ok(result);
        }
        //********************************************************************************************************//
        //get the users that bid specific artwork
        [HttpGet("{artworkId}/bids")]
        public IActionResult GetBids(int artworkId)
        {
            var bids = _context.Bid
           .Where(b => b.ArtworkId == artworkId)
           .Select(b => new
         {
            b.bid_id,
            b.amount,
            b.timeStamp,
            userName = b.User.UserName
            })
            .OrderByDescending(b => b.amount)
            .ToList();

            return Ok(bids);
        }
        //******************************************************************************************************//

        [HttpPost("close/{artworkId}")]
        public async Task<IActionResult> CloseAuction(int artworkId)
        {
            var artwork = _context.Artwork
                .Include(a => a.Bids)
                .ThenInclude(b => b.User)
                .FirstOrDefault(a => a.artworkId == artworkId);

            if (artwork == null)
                return NotFound();

            var winner = artwork.Bids
                .OrderByDescending(b => b.amount)
                .FirstOrDefault();

            if (winner == null)
                return BadRequest("No bids");

            artwork.status = ArtworkStatus.Sold;

            var notification = new Notification
            {
                userId = winner.UserId,
                message = "You won the auction!",
                isRead = false,
                createdAt = DateTime.Now
            };

            _context.Notifications.Add(notification);
            _context.SaveChanges();
            await _hub.Clients.User(winner.UserId)
       .SendAsync("ReceiveNotification", notification.message);

            return Ok(new
            {
                winner = winner.User.name,
                amount = winner.amount
            });
        }
    }
}
