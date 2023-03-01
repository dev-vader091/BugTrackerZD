using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class TicketType
    {
        // Primary Key
        public int Id { get; set; }
        [Display(Name = "Ticket Type")]
        public string? Name { get; set; }
    }
}
