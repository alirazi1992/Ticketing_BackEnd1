using System.ComponentModel.DataAnnotations;
using Ticketing.Api.Models;

namespace Ticketing.Api.Dtos
{
    public class TicketCreateRequest
    {
        [Required, MaxLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Description { get; set; } = string.Empty;

        public string Category { get; set; } = string.Empty;
        public string SubCategory { get; set; } = string.Empty;

        public TicketPriority Priority { get; set; } = TicketPriority.Medium;
    }

    public class TicketUpdateRequest
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public TicketStatus? Status { get; set; }
        public TicketPriority? Priority { get; set; }
        public int? AssignedToUserId { get; set; }
    }

    public class TicketReplyRequest
    {
        [Required]
        public string Message { get; set; } = string.Empty;

        public bool IsInternal { get; set; } = false;
    }
}
