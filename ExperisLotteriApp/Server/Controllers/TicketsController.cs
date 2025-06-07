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
        private readonly LotteriDbContext _db;

        public TicketsController(LotteriDbContext db)
        {
            _db = db;
        }

        [HttpGet("test-db")]
        public async Task<IActionResult> TestDbConnection()
        {
            try
            {
                var count = await _db.Tickets.CountAsync();
                return Ok($"Database is working. Total tickets: {count}");
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }

        [HttpGet("available-count")]
        public async Task<IActionResult> GetAvailableTicketCount()
        {
            try
            {
                var now = DateTime.UtcNow;
                var holdExpiration = now.AddMinutes(-2);

                var data = new AvailableTicketsDTO
                {
                    AvailableTickets = await AvailableTickets(holdExpiration),
                    ReservedTickets = await ReservedTickets(),
                    OnHoldTickets = await OnHoldTickets(holdExpiration)
                };

                return Ok(data);
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Database error: {ex.Message}");
            }
        }

        //[HttpGet("on-hold")]
        //public async Task<ActionResult<List<TicketDTO>>> GetOnHoldTickets()
        //{
        //    var holdExpiration = DateTime.UtcNow.AddMinutes(-2);
        //    var onHoldTickets = await _db.Tickets
        //        .Where(t => t.ControlTimestamp != null && t.ControlTimestamp >= holdExpiration && !t.IsReserved)
        //        .Select(t => new TicketDTO
        //        {
        //            Number = t.Id,
        //            IsReserved = t.IsReserved,
        //            ReservedBy = t.ReservedBy,
        //            IsWinner = t.IsWinner,
        //            ControlStamp = t.ControlTimestamp,
        //            EmployeeId = t.EmployeeId
        //        })
        //        .ToListAsync();

        //    return Ok(onHoldTickets);
        //}

        [HttpPost("buy")]
        public async Task<IActionResult> BuyTickets([FromBody] TicketReserveRequestDTO request)
        {
            var holdExpiration = DateTime.UtcNow.AddMinutes(-2);

            var tickets = await _db.Tickets
                .Where(t => request.TicketNumbers.Contains(t.Id))
                .ToListAsync();

            if (tickets.Count != request.TicketNumbers.Count)
                return BadRequest("Some tickets do not exist.");

            if (tickets.Any(t => t.IsReserved || t.ControlTimestamp == null || t.ControlTimestamp < holdExpiration))
                return BadRequest("One or more tickets are no longer available to buy.");

            foreach (var ticket in tickets)
            {
                ticket.IsReserved = true;
                ticket.ReservedBy = request.User;
            }

            await _db.SaveChangesAsync();

            return Ok();
        }

        [HttpPost("hold")]
        public async Task<IActionResult> HoldTickets([FromQuery] int count = 1)
        {
            if (count <= 0 || count > 10)
                return BadRequest("Invalid number of tickets requested.");

            var now = DateTime.UtcNow;
            var holdExpiration = now.AddMinutes(-2);

            // Phase 1: Get available ticket IDs
            var candidateTicketIds = await GetCandidateTicketIds(holdExpiration);

            if (candidateTicketIds.Count == 0)
                return BadRequest("No tickets available to hold at the moment.");

            if (candidateTicketIds.Count < count)
                return BadRequest("Not enough tickets available to hold at the moment.");

            // Phase 2: Randomly select N ticket IDs
            var selectedIds = PickRandomTicketIds(candidateTicketIds, count);

            // Phase 3: Fetch and update selected tickets
            var selectedTickets = await MarkTicketsOnHold(selectedIds, now);

            var resultList = new List<TicketDTO>();

            foreach(var selected in selectedTickets)
            {
                resultList.Add(new TicketDTO { Number = selected.Id, ControlStamp = selected.ControlTimestamp});
            }

            return Ok(resultList);
        }

        [HttpPost("draw")]
        public async Task<IActionResult> DrawWinners()
        {
            var tickets = await _db.Tickets.ToListAsync();
            if (tickets.Count < 3)
                return BadRequest("Not enough tickets to draw.");

            var rnd = new Random();
            var drawn = tickets.OrderBy(x => rnd.Next()).Take(3).ToList();

            for (int i = 0; i < drawn.Count; i++)
            {
                drawn[i].IsWinner = true;
            }

            var result = drawn.Select((t, i) => new
            {
                Prize = $"Prize {i + 1}",
                TicketNumber = t.Id,
                User = string.IsNullOrWhiteSpace(t.ReservedBy) ? "Unsold Ticket" : t.ReservedBy
            }).ToList();

            // Reset all tickets (new game)
            foreach (var ticket in tickets)
            {
                ticket.IsReserved = false;
                ticket.ReservedBy = null;
                ticket.ControlTimestamp = null;
                ticket.IsWinner = false;
            }

            await _db.SaveChangesAsync();

            return Ok(result);
        }

        private async Task<int> AvailableTickets(DateTime holdExpiration)
        {
            int availableCount = await _db.Tickets.CountAsync(t =>
                !t.IsReserved &&
                (t.ControlTimestamp == null || t.ControlTimestamp < holdExpiration)
            );

            return availableCount;
        }

        private async Task<int> ReservedTickets()
        {
            int reservedCount = await _db.Tickets.CountAsync(t => t.IsReserved);

            return reservedCount;
        }

        private async Task<int> OnHoldTickets(DateTime holdExpiration)
        {
            int onHoldCount = await _db.Tickets.CountAsync(t =>
                !t.IsReserved && t.ControlTimestamp > holdExpiration
            );

            return onHoldCount;
        }

        private async Task<List<int>> GetCandidateTicketIds(DateTime holdExpiration)
        {
            return await _db.Tickets
                .Where(t => !t.IsReserved && (t.ControlTimestamp == null || t.ControlTimestamp < holdExpiration))
                .Select(t => t.Id)
                .ToListAsync();
        }

        private List<int> PickRandomTicketIds(List<int> candidateIds, int count)
        {
            var random = new Random();
            return candidateIds
                .OrderBy(_ => random.Next())
                .Take(count)
                .ToList();
        }

        private async Task<List<Ticket>> MarkTicketsOnHold(List<int> selectedIds, DateTime now)
        {
            var tickets = await _db.Tickets
                .Where(t => selectedIds.Contains(t.Id))
                .ToListAsync();

            foreach (var ticket in tickets)
            {
                ticket.ControlTimestamp = now;
            }

            await _db.SaveChangesAsync();
            return tickets;
        }
    }
}
