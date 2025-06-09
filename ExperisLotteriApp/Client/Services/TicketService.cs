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

        public async Task<List<TicketDTO>> HoldTicketsAsync(int count)
        {
            var response = await _http.PostAsync($"api/tickets/hold?count={count}", null);
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<List<TicketDTO>>();
                return result ?? new List<TicketDTO>();
            }

            throw new Exception(await response.Content.ReadAsStringAsync());
        }

        public async Task<bool> BuyTicketsAsync(List<int> ticketNumbers, string name)
        {
            var request = new TicketReserveRequestDTO
            {
                User = name,
                TicketNumbers = ticketNumbers
            };

            var response = await _http.PostAsJsonAsync("api/tickets/buy", request);
            return response.IsSuccessStatusCode;
        }

        public async Task<List<WinnerResultDTO>> DrawWinnersAsync()
        {
            var result = await _http.PostAsJsonAsync("api/tickets/draw", new { });

            if (result.IsSuccessStatusCode)
            {
                return await result.Content.ReadFromJsonAsync<List<WinnerResultDTO>>() ?? new();
            }

            throw new Exception("Could not draw winners");
        }
    }
}
