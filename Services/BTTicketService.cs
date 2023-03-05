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
        public async Task<Ticket> GetTicketAsync(int? id)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                              .Include(t => t.Project)
                                              .Include(t => t.DeveloperUser)
                                              .Include(t => t.SubmitterUser)
                                              .Include(t => t.TicketPriority)
                                              .Include(t => t.TicketStatus)
                                              .Include(t => t.TicketType)
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

    }
}
