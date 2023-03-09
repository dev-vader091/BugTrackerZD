using BugHunterBugTrackerZD.Models;

namespace BugHunterBugTrackerZD.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task AddTicketAsync(Ticket ticket);
        public Task UpdateTicketAsync(Ticket ticket);

        public Task<Ticket> GetTicketByIdAsync(int? id);
        public Task DeleteTicketAsync(Ticket ticket);
    }
}
