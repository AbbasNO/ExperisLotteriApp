using Shared.DTO;
using System.Net.Http.Json;

namespace Client.Services
{
    public class TicketService : ITicketService
    {
        private readonly HttpClient _http;

        public TicketService(HttpClient http)
        {
            _http = http;
        }

        public async Task<AvailableTicketsDTO> GetAvailableCountAsync()
        {
            var result = await _http.GetFromJsonAsync<AvailableTicketsDTO>("api/tickets/available-count");
            return result ?? new AvailableTicketsDTO();
        }

        public async Task<List<TicketDTO>> GetAllTicketsAsync()
        {
            var tickets = await _http.GetFromJsonAsync<List<TicketDTO>>("api/tickets");
            return tickets ?? new List<TicketDTO>();
        }
    }
}
