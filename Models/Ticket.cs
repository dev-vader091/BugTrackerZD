﻿using System.ComponentModel.DataAnnotations;
using System.Net.Mail;

namespace BugHunterBugTrackerZD.Models
{
    public class Ticket
    {
        // Primary Key 
        public int Id { get; set; }
        [Required]
        [StringLength(50, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Title { get; set; }
        [Required]
        [StringLength(600, ErrorMessage = "The {0} must be at least {2} and at most {1} characters", MinimumLength = 2)]
        public string? Description { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime? Updated { get; set; }
        public bool Archived { get; set; }
        public bool ArchivedByProject { get; set;}
        // Foreign Keys
        public int ProjectId { get; set; }
        public int TicketTypeId { get; set; }
        public int TickStatusId { get; set; }
        public int TicketPriorityId { get; set; }
        public string? DeveloperUserId { get; set; }
        [Required]
        public string? SubmitterUserId { get; set; }

        // Navigation Properties
        public virtual Project? Project { get; set; }
        public virtual TicketPriority? TicketPriority { get; set; }
        public virtual TicketType? TicketType { get; set; }
        public virtual TicketStatus? TicketStatus { get; set; }
        public virtual BTUser? DeveloperUser { get; set; }
        public virtual BTUser? SubmitterUser { get; set; }
        public virtual ICollection<TicketComment> Comments { get; set; } = new HashSet<TicketComment>();
        public virtual ICollection<Attachment> Attachments { get; set; } = new HashSet<Attachment>();
        public virtual ICollection<TicketHistory>? History { get; set; } = new HashSet<TicketHistory>();


    }
}
