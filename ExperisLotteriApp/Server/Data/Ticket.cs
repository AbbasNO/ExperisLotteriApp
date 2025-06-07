using System.ComponentModel.DataAnnotations.Schema;
using System.ComponentModel.DataAnnotations;

namespace Server.Data
{
    [Table("Tickets")]
    public class Ticket
    {
        [Key]
        [Column("ID")]
        public int Id { get; set; }

        [Column("IS_RESERVED")]
        public bool IsReserved { get; set; }

        [Column("RESERVED_BY")]
        [MaxLength(50)]
        public string? ReservedBy { get; set; }

        [Column("IS_WINNER")]
        public bool IsWinner { get; set; }

        [Column("CONTROL_TIMESTAMP")]
        public DateTime? ControlTimestamp { get; set; }

        [Column("EMPLOYEE_ID")]
        public int? EmployeeId { get; set; }
    }
}
