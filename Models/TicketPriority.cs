using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class TicketPriority
    {
        // Primary Key
        public int Id { get; set; }
        [Display(Name = "Ticket Priority")]
        [StringLength(30, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]

        public string? Name { get; set; }
    }
}
