using Microsoft.AspNetCore.Mvc.Rendering;

namespace BugHunterBugTrackerZD.Models.ViewModels
{
    public class TicketUsersViewModel
    {
        public Ticket? Ticket { get; set; }
        public SelectList? TicketUsers { get; set; }

        public string? SelectedDeveloperId { get; set; }

        //public BTUser? Developer { get; set; }
    }
}
