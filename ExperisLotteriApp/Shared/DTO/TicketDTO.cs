using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class TicketDTO
    {
        public int Number { get; set; }
        public bool IsReserved { get; set; }
        public string? ReservedBy { get; set; }
        public bool IsWinner { get; set; } = false;
        public DateTime? ControlStamp { get; set; }
        public int? EmployeeId { get; set; }

    }
}
