using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class TicketComment
    {
        // Primary Key 
        public int Id { get; set; }
        [Required]
        [StringLength(250, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Comment { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        // Foreign Keys
        public int TicketId { get; set; }
        [Required]
        public string? UserId { get; set; }

        // Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }
    }
}
