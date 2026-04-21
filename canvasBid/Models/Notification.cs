using System.ComponentModel.DataAnnotations;
namespace canvasBid.Models
{
    public class Notification
    {
        [Key]
        public int notificationId { get; set; }

        public String message { get; set; } = "";

        public Boolean isRead { get; set; }
        public DateTime createdAt { get; set; }
        public string userId { get; set; }

        public User user { get; set; }
    }
}
