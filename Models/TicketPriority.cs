using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class TicketPriority
    {
        // Primary Key
        public int Id { get; set; }
        [Display(Name = "Priority")]
        public string? Name { get; set; }
    }
}
