﻿using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugHunterBugTrackerZD.Models
{
    public class Company
    {
        // Primary Key
        public int Id { get; set; }
        [Required]
        [Display(Name = "Company")]
        public string? Name { get; set; }
        public string? Description { get; set; }

        // Image Data
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }
        public string? ImageFileType { get; set; }

        // Navigation Properties
        public virtual ICollection<Project> Projects { get; set; } = new HashSet<Project>();
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Invite> Invites { get; set; } = new HashSet<Invite>();
    }
}
