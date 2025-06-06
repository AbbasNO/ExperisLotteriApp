using Shared.DTO;

namespace Client.Services
{
    public interface ITicketService
    {
        Task<AvailableTicketsDTO> GetAvailableCountAsync();
        Task<List<TicketDTO>> GetAllTicketsAsync();
    }
}
