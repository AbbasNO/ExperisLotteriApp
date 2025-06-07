using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class AvailableTicketsDTO
    {
        public int AvailableTickets { get; set; }
        public int ReservedTickets { get; set; }
        public int OnHoldTickets { get; set; }
    }
}
