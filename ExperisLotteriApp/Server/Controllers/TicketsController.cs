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
                int availableCount = await db.Tickets.CountAsync(t => !t.IsReserved);
                int ReservedCount = await db.Tickets.CountAsync(t => t.IsReserved);

                var data = new AvailableTicketsDTO() { AvailableTickets = availableCount, ReservedTickets = ReservedCount };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }

    }
}
