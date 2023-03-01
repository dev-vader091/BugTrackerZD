using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class TicketHistory
    {
        // Primary Key
        public int Id { get; set; }
        // Foreign Key
        public int TicketId { get; set; }
        [Display(Name = "Name")]
        public string? PropertyName { get; set; }
        public string? Description { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        public string? OldValue { get; set; }
        public string? NewValue { get; set;}
        public int UserId { get; set; }

        // Navigation Properties
        public virtual Ticket? Ticket { get; set; }
        public virtual BTUser? User { get; set; }

    }
}
