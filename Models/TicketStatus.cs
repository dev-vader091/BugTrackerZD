using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class TicketStatus
    {
        // Primary Key
        public int Id { get; set; }
        [Display(Name = "Status")]
        public string? Name { get; set; }
    }
}
