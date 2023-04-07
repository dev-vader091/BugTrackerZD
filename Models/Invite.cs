using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace BugHunterBugTrackerZD.Models
{
    public class Invite
    {
        // Primary Key 
        public int Id { get; set; }

        [DataType(DataType.Date)]
        public DateTime InviteDate { get; set; }

        [DataType(DataType.Date)]
        public DateTime? JoinDate { get; set; }

        public Guid CompanyToken { get; set; }

        // Foreign Keys
        public int CompanyId { get; set; }

        public int ProjectId { get; set; }

        [Required]
        public string? InvitorId { get; set; }

        public string? InviteeId { get; set; }

        // Foreign Keys End
        [Required]
        [Display(Name = "Invited Email")]
        public string? InviteeEmail { get; set; }

        [Required]
        [Display(Name = "Invited First Name")]
        public string? InviteeFirstName { get; set; }

        [Required]
        [Display(Name = "Invited Last Name")]
        public string? InviteeLastName { get; set; }

        [NotMapped]
        [Display(Name = "Invited Name")]
        public string? InviteeFullName { get { return $"{InviteeFirstName} {InviteeLastName}"; } }

        public string? Message { get; set; }

        public bool IsValid { get; set; }

        // Navigation Properties
        public virtual Company? Company { get; set; }

        public virtual Project? Project { get; set; }

        public virtual BTUser? Invitor { get; set; }

        public virtual BTUser? Invitee { get; set; }
    }
}
