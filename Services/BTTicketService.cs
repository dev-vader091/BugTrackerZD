using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugHunterBugTrackerZD.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;


        public BTTicketService(ApplicationDbContext context)
        {
            _context = context;
        }
        public async Task AddTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Add(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public async Task<Ticket> GetTicketByIdAsync(int? id, int? companyId)
            
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                              .Where(t => t.Project!.CompanyId == companyId)
                                              .Include(t => t.Project)
                                                .ThenInclude(p => p!.Members)
                                              .Include(t => t.DeveloperUser)
                                              .Include(t => t.SubmitterUser)
                                              .Include(t => t.TicketPriority)
                                              .Include(t => t.TicketStatus)
                                              .Include(t => t.TicketType)
                                              .Include(t => t.Comments)
                                                .ThenInclude(c => c.User) 
                                              .FirstOrDefaultAsync(ticket => ticket.Id == id);
                                    




                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateTicketAsync(Ticket ticket)
        {
            try
            {
                _context.Update(ticket);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }
        public Task DeleteTicketAsync(Ticket ticket)
        {
            throw new NotImplementedException();
        }

        public async Task<List<Ticket>> GetTicketsByIdAsync(int? companyId)
        {
            try
            {
                List<Ticket> tickets = new();

                tickets = await _context.Tickets
                                    .Where(t => t.Project!.CompanyId == companyId)
                                    .Include(t => t.Project)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.SubmitterUser)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<Ticket>> GetUnassignedTicketsAsync(int? companyId)
        {
            try
            {
                List<Ticket> tickets = new();

                tickets = await _context.Tickets
                                    .Where(t => t.Project!.CompanyId == companyId && t.DeveloperUserId == null)
                                    .Include(t => t.Project)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.SubmitterUser)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .ToListAsync();

                return tickets;


            }
            catch (Exception)
            {

                throw;
            }
        }

        //public async Task RemoveTicketDeveloperAsync(int? ticketId)
        //{
        //    try
        //    {
        //        Ticket? ticket = await _context.Tickets.Include(t => t.Project).FirstOrDefaultAsync(t => t.Id == ticketId);

        //        if (ticket!.DeveloperUser != null)
        //        {
        //            ticket.DeveloperUser = null;
        //        }
        //    }
        //    catch (Exception)
        //    {

        //        throw;
        //    }
        //}
    }
}
