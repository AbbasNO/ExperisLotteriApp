using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Shared.DTO
{
    public class WinnerResultDTO
    {
        public string Prize { get; set; } = "";
        public int TicketNumber { get; set; }
        public string User { get; set; } = "";
    }
}
