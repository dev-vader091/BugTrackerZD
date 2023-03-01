using Microsoft.Data.SqlClient;
using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class Notification
    {
        // Primary Key
        public int Id { get; set; }
        // Foreign Keys
        public int ProjectId { get; set; }
        public int TicketId { get; set; }
        // Foreign Key End
        [Required]
        public string? Title { get; set; }
        [Required]
        [StringLength(600, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Message { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }

        // Foreign Keys
        [Required]
        public string? SenderId { get; set; }
        [Required]
        public string? RecipientId { get; set; }
        // Foreign Keys End
        public int NotificationTypeId { get; set; }
        public bool HasBeenViewed { get; set; }

        // Navigation Properties 
        public virtual NotificationType? NotificationType { get; set; }
        public virtual Ticket? Ticket { get; set; }
        public virtual Project? Project { get; set; }
        public virtual BTUser? Sender { get; set; }
        public virtual BTUser? Recipient { get; set; }
    }
}
