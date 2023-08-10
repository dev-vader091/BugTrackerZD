using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugHunterBugTrackerZD.Services
{
    public class BTTicketService : IBTTicketService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;


        public BTTicketService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
        }

        // Section: CRUD Operations
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
                                              .Include(t => t.History)
                                              .Include(t => t.Attachments)
                                              .Include(t => t.Comments)
                                                .ThenInclude(c => c.User) 
                                              .FirstOrDefaultAsync(t => t.Id == id);
                 
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

        // Section End: CRUD Operations

        public async Task<List<Ticket>> GetArchivedTicketsAsync(int? companyId)
        {
            try
            {
                List<Ticket> tickets = new();

                tickets = await _context.Tickets
                                    .Where(t => t.Project!.CompanyId == companyId && t.Archived == true)
                                    .Include(t => t.Project)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.SubmitterUser)
                                    .Include(t => t.TicketPriority)
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .ToListAsync();

                foreach (Ticket ticket in tickets)
                {
                    if (ticket.Project != null && ticket.Project.Archived == true)
                    {
                        ticket.ArchivedByProject = true;
                        await _context.SaveChangesAsync();
                    }
                }

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
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

        public async Task AddTicketAttachmentAsync(TicketAttachment ticketAttachment)
        {
            try
            {
                await _context.AddAsync(ticketAttachment);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<TicketAttachment> GetTicketAttachmentByIdAsync(int ticketAttachmentId)
        {
            try
            {
                TicketAttachment? ticketAttachment = await _context.Attachments
                                                                  .Include(t => t.BTUser)
                                                                  .FirstOrDefaultAsync(t => t.Id == ticketAttachmentId);
                return ticketAttachment!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> GetTicketAsNoTrackingAsync(int? ticketId, int? companyId)
        {
            try
            {
                Ticket? ticket = await _context.Tickets
                                                .Include(t => t.Project)
                                                    .ThenInclude(p => p!.Company)
                                                .Include(t => t.Attachments)
                                                .Include(t => t.Comments)
                                                .Include(t => t.DeveloperUser)
                                                .Include(t => t.History)
                                                .Include(t => t.SubmitterUser)
                                                .Include(t => t.TicketPriority)
                                                .Include(t => t.TicketStatus)
                                                .Include(t => t.TicketType)
                                                .AsNoTracking()
                                                .FirstOrDefaultAsync(t => t.Id == ticketId && t.Project!.CompanyId == companyId && t.Archived == false);
                return ticket!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Ticket> AssignTicketToDeveloperAsync(int? ticketId, string? developerId, int? companyId)
        {
            Ticket? ticket = await GetTicketByIdAsync(ticketId, companyId);

            ticket.DeveloperUserId = developerId;
            await _context.SaveChangesAsync();

            return ticket;
        }

        public async Task<List<Ticket>> GetTicketsByProjectAsync(int? projectId)
        {
            try
            {
                List<Ticket> tickets = await _context.Tickets.Where(t => t.ProjectId == projectId)
                                                         .Include(t => t.Project)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.SubmitterUser)
                                                         .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }
    }
}
