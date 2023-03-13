using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace BugHunterBugTrackerZD.Services.Interfaces
{
    public interface IBTTicketService
    {
        public Task AddTicketAsync(Ticket ticket);

        public Task UpdateTicketAsync(Ticket ticket);

        public Task<Ticket> GetTicketByIdAsync(int? id, int? companyId);

        public Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId);
        
        public Task<List<Ticket>> GetTicketsByIdAsync(int? companyId);

        public Task<List<Ticket>> GetUnassignedTicketsAsync(int? companyId);

        public Task DeleteTicketAsync(Ticket ticket);

        public Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment);

        public Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId);
    }
}
