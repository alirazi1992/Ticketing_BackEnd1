using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Ticketing.Api.Data;
using Ticketing.Api.Dtos;
using Ticketing.Api.Models;

namespace Ticketing.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class TicketsController : ControllerBase
    {
        private readonly TicketingDbContext _db;

        public TicketsController(TicketingDbContext db)
        {
            _db = db;
        }

        private int? GetCurrentUserId()
        {
            var idString = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (int.TryParse(idString, out var id))
                return id;
            return null;
        }

        private bool IsAdmin => User.IsInRole(UserRole.Admin.ToString());
        private bool IsTechnician => User.IsInRole(UserRole.Technician.ToString());
        private bool IsClient => User.IsInRole(UserRole.Client.ToString());

        // GET: api/tickets/my
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Ticket>>> GetMyTickets()
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var query = _db.Tickets
                .Include(t => t.Messages)
                .OrderByDescending(t => t.CreatedAt)
                .AsQueryable();

            if (IsClient)
            {
                query = query.Where(t => t.CreatedByUserId == userId);
            }
            else if (IsTechnician)
            {
                query = query.Where(t => t.AssignedToUserId == userId);
            }
            else if (IsAdmin)
            {
                // admin sees all tickets
            }

            var tickets = await query.ToListAsync();
            return Ok(tickets);
        }

        // GET: api/tickets/{id}
        [HttpGet("{id:int}")]
        public async Task<ActionResult<Ticket>> GetTicket(int id)
        {
            var ticket = await _db.Tickets
                .Include(t => t.Messages)
                .FirstOrDefaultAsync(t => t.Id == id);

            if (ticket == null) return NotFound();

            return Ok(ticket);
        }

        // POST: api/tickets
        [HttpPost]
        [Authorize(Roles = "Client,Admin")]
        public async Task<ActionResult<Ticket>> CreateTicket(TicketCreateRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var ticket = new Ticket
            {
                Title = request.Title,
                Description = request.Description,
                Category = request.Category,
                SubCategory = request.SubCategory,
                Priority = request.Priority,
                Status = TicketStatus.Open,
                CreatedByUserId = userId.Value,
                CreatedAt = DateTime.UtcNow
            };

            _db.Tickets.Add(ticket);
            await _db.SaveChangesAsync();

            return CreatedAtAction(nameof(GetTicket), new { id = ticket.Id }, ticket);
        }

        // PUT: api/tickets/{id}
        [HttpPut("{id:int}")]
        [Authorize(Roles = "Admin,Technician")]
        public async Task<ActionResult> UpdateTicket(int id, TicketUpdateRequest request)
        {
            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            if (request.Title != null) ticket.Title = request.Title;
            if (request.Description != null) ticket.Description = request.Description;
            if (request.Status.HasValue) ticket.Status = request.Status.Value;
            if (request.Priority.HasValue) ticket.Priority = request.Priority.Value;
            if (request.AssignedToUserId.HasValue)
                ticket.AssignedToUserId = request.AssignedToUserId.Value;

            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return NoContent();
        }

        // POST: api/tickets/{id}/replies
        [HttpPost("{id:int}/replies")]
        public async Task<ActionResult> AddReply(int id, TicketReplyRequest request)
        {
            var userId = GetCurrentUserId();
            if (userId == null) return Unauthorized();

            var ticket = await _db.Tickets.FindAsync(id);
            if (ticket == null) return NotFound();

            var message = new TicketMessage
            {
                TicketId = ticket.Id,
                SenderUserId = userId.Value,
                Message = request.Message,
                IsInternal = request.IsInternal,
                CreatedAt = DateTime.UtcNow
            };

            _db.TicketMessages.Add(message);
            ticket.UpdatedAt = DateTime.UtcNow;

            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}
