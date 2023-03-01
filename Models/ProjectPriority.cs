using System.ComponentModel.DataAnnotations;

namespace BugHunterBugTrackerZD.Models
{
    public class ProjectPriority
    {
        // Primary Key
        public int Id { get; set; }
        [Display(Name = "Priority")]
        public string? Name { get; set; }
    }
}
