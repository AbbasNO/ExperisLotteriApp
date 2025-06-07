using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.DTO;
using System.Net.Sockets;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDbConnection([FromServices] LotteriDbContext db)
        {
            try
            {
                var count = await db.Tickets.CountAsync();
                return Ok($"Database is working. Total tickets: {count}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }

        [HttpGet("available-count")]
        public async Task<IActionResult> GetAvailableTicketCount([FromServices] LotteriDbContext db)
        {
            try
            {
                var now = DateTime.UtcNow;
                var holdExpiration = now.AddMinutes(-2);

                var data = new AvailableTicketsDTO
                {
                    AvailableTickets = await AvailableTickets(db, holdExpiration),
                    ReservedTickets = await ReservedTickets(db),
                    OnHoldTickets = await OnHoldTickets(db, holdExpiration)
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }

        //[HttpPost("hold/{numberOfTickets}")]
        //public async Task<IActionResult> HoldTicket(int numberOfTickets, [FromServices] LotteriDbContext db)
        //{
        //    var ticket = await db.Tickets.Where(t => t.ControlTimestamp == null).;

        //    if (ticket == null || ticket.IsReserved)
        //        return NotFound("Ticket not found or already reserved.");

        //    ticket.ControlTimestamp = DateTime.UtcNow;
        //    await db.SaveChangesAsync();

        //    return Ok("Hold timestamp updated.");
        //}

        private async Task<int> AvailableTickets(LotteriDbContext db, DateTime holdExpiration)
        {
            int availableCount = await db.Tickets.CountAsync(t =>
                !t.IsReserved &&
                (t.ControlTimestamp == null || t.ControlTimestamp < holdExpiration)
            );

            return availableCount;
        }

        private async Task<int> ReservedTickets(LotteriDbContext db)
        {
            int reservedCount = await db.Tickets.CountAsync(t => t.IsReserved);

            return reservedCount;
        }

        private async Task<int> OnHoldTickets(LotteriDbContext db, DateTime holdExpiration)
        {
            int onHoldCount = await db.Tickets.CountAsync(t =>
                !t.IsReserved && t.ControlTimestamp > holdExpiration
            );

            return onHoldCount;
        }
    }
}
