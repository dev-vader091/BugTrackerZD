﻿using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class TicketComment
    {
        // Primary Key 
        public int Id { get; set; }
        [Required]
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
