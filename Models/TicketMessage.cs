using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace Ticketing.Api.Models
{
    public class TicketMessage
    {
        public int Id { get; set; }

        [Required]
        public string Message { get; set; } = string.Empty;

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public bool IsInternal { get; set; } = false;

        // Relations
        public int TicketId { get; set; }

        // ❗ Ignore the back reference when sending JSON to the client
        [JsonIgnore]
        public Ticket? Ticket { get; set; }

        public int SenderUserId { get; set; }
        public User? SenderUser { get; set; }
    }
}
