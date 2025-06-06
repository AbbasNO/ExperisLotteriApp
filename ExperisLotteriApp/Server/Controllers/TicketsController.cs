using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR.Protocol;
using Microsoft.EntityFrameworkCore;
using Server.Data;
using Shared.DTO;
using Shared.Models;
using System.Net.Sockets;

namespace Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TicketsController : ControllerBase
    {
        private static readonly object _lock = new();
        private static readonly List<Shared.Models.Ticket> Tickets = Enumerable.Range(1, 100).Select(i => new Shared.Models.Ticket { Number = i }).ToList();
        private static readonly List<Shared.Models.Ticket> Winners = new();

        [HttpGet]
        public ActionResult<List<Shared.Models.Ticket>> GetAll() => Tickets;

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


        [HttpPost("reserve")]
        [Authorize]
        public IActionResult Reserve([FromBody] TicketReserveRequest request)
        {
            lock (_lock)
            {
                foreach (var number in request.TicketNumbers)
                {
                    var t = Tickets.FirstOrDefault(t => t.Number == number);
                    if (t == null || t.IsReserved)
                        return BadRequest($"Ticket {number} is already taken.");
                }

                foreach (var number in request.TicketNumbers)
                {
                    var t = Tickets.First(t => t.Number == number);
                    t.IsReserved = true;
                    t.ReservedBy = request.User;
                }
                return Ok();
            }
        }

        [HttpPost("draw")]
        [Authorize]
        public ActionResult<DrawResult> DrawWinners([FromQuery] int numberOfWines = 5)
        {
            lock (_lock)
            {
                var eligible = Tickets.Where(t => t.IsReserved && !t.IsWinner).ToList();
                if (eligible.Count < numberOfWines)
                    return BadRequest("Not enough reserved tickets to draw the requested number of winners.");

                var rnd = new Random();
                var drawn = eligible.OrderBy(_ => rnd.Next()).Take(numberOfWines).ToList();

                foreach (var winner in drawn)
                {
                    winner.IsWinner = true;
                    Winners.Add(winner);
                }

                return Ok(new DrawResult { Winners = drawn });
            }
        }
    }
}
