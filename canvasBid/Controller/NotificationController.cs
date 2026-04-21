using canvasBid.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace canvasBid.Controller
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class NotificationController: ControllerBase
    {
        private readonly dbContext _context;

        public NotificationController(dbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public IActionResult GetNotifications()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);

            var noti = _context.Notifications
                .Where(n => n.userId == userId)
                .ToList();

            return Ok(noti);
        }
    }
}
