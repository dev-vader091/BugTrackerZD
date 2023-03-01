using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugHunterBugTrackerZD.Models
{
    public class Project
    {
        // Primary Key 
        public int Id { get; set; }
        // Foreign Key
        public int CompanyId { get; set; }
        [Required]
        [Display(Name = "Project Name")]
        public string? Name { get; set; }
        [Required]
        public string? Description { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime Created { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime StartDate { get; set; }
        [DataType(DataType.DateTime)]
        public DateTime EndDate { get; set; }
        public int ProjectPriorityId { get; set; }
        // Image Data
        [NotMapped]
        public IFormFile? ImageFormFile { get; set; }
        public byte[]? ImageFileData { get; set; }

        public string? ImageFileType { get; set; }
        public bool Archived { get; set; }

        // Navigation Properties
        public virtual Company? Company { get; set; }
        public virtual ProjectPriority? ProjectPriority { get; set; }
        public virtual ICollection<BTUser> Members { get; set; } = new HashSet<BTUser>();
        public virtual ICollection<Ticket> Tickets { get; set; } = new HashSet<Ticket>();




    }
}
