using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class TicketReserveRequestDTO
    {
        public string User { get; set; } = string.Empty;
        public List<int> TicketNumbers { get; set; } = new();
        public bool FewTicketsLeft { get; set; }
    }
}
