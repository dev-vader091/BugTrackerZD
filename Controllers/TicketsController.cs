using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using BugHunterBugTrackerZD.Models.Enums;
using BugHunterBugTrackerZD.Services.Interfaces;
using BugHunterBugTrackerZD.Extensions;
using BugHunterBugTrackerZD.Models.ViewModels;
using BugHunterBugTrackerZD.Services;
using System.ComponentModel.Design;

namespace BugHunterBugTrackerZD.Controllers
{
    [Authorize]
    public class TicketsController : Controller
    {

        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTTicketService _ticketService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTTicketHistoryService _historyService;

        public TicketsController(ApplicationDbContext context, 
                                 UserManager<BTUser> userManager, 
                                 IBTTicketService ticketService, 
                                 IBTRolesService rolesService, 
                                 IBTTicketHistoryService historyService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _historyService = historyService;
        }

        

        // POST: Add Ticket Comment 
        public async Task<IActionResult> AddTicketComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                string? userId = _userManager.GetUserId(User);

                ticketComment.UserId = userId;
                ticketComment.Created = DataUtility.GetPostGresDate(DateTime.Now);


                _context.Add(ticketComment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            //ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
            //ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            
            return View(ticketComment);

           
        }
        
        // GET: Assign Ticket 
        [HttpGet]
        public async Task<IActionResult> AssignTicketToUser(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);

            List<BTUser> projectMembers = ticket.Project!.Members.ToList();

            List<BTUser> developers = new();

            foreach (BTUser user in projectMembers)
            {
                if (await _rolesService.IsUserInRoleAsync(user, nameof(BTRoles.Developer)))
                {
                    developers.Add(user);
                }
            }

            BTUser? currentDev = ticket.DeveloperUser;

            TicketUsersViewModel viewModel = new()
            {
                Ticket = ticket,
                TicketUsers = new SelectList(developers, "Id", "FullName", currentDev?.Id),
                SelectedDeveloperId = currentDev?.Id
                
            };

            return View(viewModel);
        }

        // POST: Assign Ticket 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignTicketToUser(TicketUsersViewModel viewModel)
        {
            int companyId = User.Identity!.GetCompanyId();

            Ticket? ticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket!.Id, companyId);

            if (!string.IsNullOrEmpty(viewModel.SelectedDeveloperId))
            {

                // Add developerId to ticket
                ticket!.DeveloperUserId = viewModel.SelectedDeveloperId;

                //_context.Update(ticket);
                await _context.SaveChangesAsync();

                return RedirectToAction("Details", new { id = viewModel.Ticket?.Id });

            }

            return View(viewModel);
        }

        // GET: Unassigned Tickets
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> UnassignedTickets()
        {
            string? userId = _userManager.GetUserId(User);
            BTUser? btUser = await _context.Users
                                           .Include(u => u.Projects)
                                             .ThenInclude(p => p.Tickets)
                                             .ThenInclude(t => t.DeveloperUser)
                                           .Include(u => u.Projects)
                                             .ThenInclude(p => p.Tickets)
                                             .ThenInclude(t => t.SubmitterUser)
                                           .Include(u => u.Projects)
                                             .ThenInclude(p => p.Tickets)
                                             .ThenInclude(t => t.TicketPriority)
                                           .Include(u => u.Projects)
                                             .ThenInclude(p => p.Tickets)
                                             .ThenInclude(t => t.TicketStatus)
                                           .Include(u => u.Projects)
                                             .ThenInclude(p => p.Tickets)
                                             .ThenInclude(t => t.TicketType)
                                           .FirstOrDefaultAsync(u => u.Id == userId);

            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Ticket> tickets = new List<Ticket>();

            if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.Admin)))
            {
                tickets = await _ticketService.GetUnassignedTicketsAsync(companyId);

            }

            else if (await _rolesService.IsUserInRoleAsync(btUser!, nameof(BTRoles.ProjectManager)))
            {
                List<Ticket>? pmTickets = new();

                pmTickets = btUser?.Projects.SelectMany(p => p.Tickets).ToList();

                tickets = pmTickets!.Where(t => t.DeveloperUserId == null);                                         
            }



            return View(tickets);
        
        }
        // GET: Tickets
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Ticket> tickets = new List<Ticket>();

            tickets = await _ticketService.GetTicketsByIdAsync(companyId);

            return View(tickets);
            
        }

        // GET: My Tickets
        public async Task<IActionResult> MyTickets()
        {
           BTUser? user = await _userManager.GetUserAsync(User);

            List<Ticket> tickets = new List<Ticket>();

            tickets = await _context.Tickets
                                    .Where(t => t.DeveloperUserId == user!.Id || t.SubmitterUserId == user.Id && t.Project!.CompanyId == user.CompanyId)
                                    .Include(t => t.Project)
                                    .Include(t => t.DeveloperUser)
                                    .Include(t => t.SubmitterUser)
                                    .Include(t => t.TicketPriority)                                    
                                    .Include(t => t.TicketStatus)
                                    .Include(t => t.TicketType)
                                    .ToListAsync();
            
            return View(tickets);
        }

        // GET: Tickets/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);
            
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // GET: Tickets/Create
        public async Task<IActionResult> Create()
        {
            BTUser? user = await _userManager.GetUserAsync(User);

            IEnumerable<Project> projects = new List<Project>();

            projects = await _context.Projects
                                     .Where(p => p.CompanyId == user!.CompanyId)
                                     .Include(p => p.Company)
                                     .Include(p => p.Tickets)
                                     .Include(p => p.ProjectPriority)
                                     .ToListAsync();

            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["ProjectId"] = new SelectList(projects, "Id", "Name");
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id");
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name");
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name");
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name");
            return View();
        }

        

        // POST: Tickets/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            ModelState.Remove("SubmitterUserId");
            if (ModelState.IsValid)
            {
                BTUser? user = await _userManager.GetUserAsync(User);

                ticket.Created = DataUtility.GetPostGresDate(DateTime.Now);
                ticket.SubmitterUserId = user!.Id;


                //if (User.IsInRole(BTRoles.Developer.ToString()))
                //{
                //    ticket.DeveloperUserId = user!.Id;
                //}


                //if (!User.IsInRole("Developer"))
                //{
                //    ticket.DeveloperUserId = null;
                //}

                await _ticketService.AddTicketAsync(ticket);

                int companyId = User.Identity!.GetCompanyId();
                Ticket? newTicket = await _context.Tickets
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
                                                .FirstOrDefaultAsync(t => t.Id == ticket.Id && t.Project!.CompanyId == companyId && t.Archived == false);

                await _historyService.AddHistoryAsync(null, newTicket, user.Id);                                  
                
                return RedirectToAction(nameof(Index));
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);

            //var ticket = await _context.Tickets.FindAsync(id);
            if (ticket == null)
            {
                return NotFound();
            }
            ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Name", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Name", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Name", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Name", ticket.TicketTypeId);
            return View(ticket);
        }

        // POST: Tickets/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            //ModelState.Remove("SubmitterUserId");
            if (ModelState.IsValid)
            {
                BTUser? user = await _userManager.GetUserAsync(User);
                try
                {
                    //ticket.SubmitterUserId = user!.Id;
                    int companyId = User.Identity!.GetCompanyId();

                    Ticket? oldTicket = await _context.Tickets
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
                                                .FirstOrDefaultAsync(t => t.Id == ticket.Id && t.Project!.CompanyId == companyId && t.Archived == false);

                    // Reformat Created/Updated Dates
                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                   
                    await _ticketService.UpdateTicketAsync(ticket);

                    Ticket? newTicket = await _context.Tickets
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
                                                .FirstOrDefaultAsync(t => t.Id == ticket.Id && t.Project!.CompanyId == companyId && t.Archived == false);

                    await _historyService.AddHistoryAsync(oldTicket, newTicket, user!.Id);

                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!TicketExists(ticket.Id))
                    {
                        return NotFound();
                    }
                    else
                    {
                        throw;
                    }
                }
                return RedirectToAction(nameof(Index));
            }
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
            ViewData["ProjectId"] = new SelectList(_context.Projects, "Id", "Description", ticket.ProjectId);
            //ViewData["SubmitterUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.SubmitterUserId);
            ViewData["TicketPriorityId"] = new SelectList(_context.TicketPriorities, "Id", "Id", ticket.TicketPriorityId);
            ViewData["TicketStatusId"] = new SelectList(_context.TicketStatuses, "Id", "Id", ticket.TicketStatusId);
            ViewData["TicketTypeId"] = new SelectList(_context.TicketTypes, "Id", "Id", ticket.TicketTypeId);
            return View(ticket);
        }

        // GET: Tickets/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Tickets == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);


            //var ticket = await _context.Tickets
            //    .Include(t => t.DeveloperUser)
            //    .Include(t => t.Project)
            //    .Include(t => t.SubmitterUser)
            //    .Include(t => t.TicketPriority)
            //    .Include(t => t.TicketStatus)
            //    .Include(t => t.TicketType)
            //    .FirstOrDefaultAsync(m => m.Id == id);
            if (ticket == null)
            {
                return NotFound();
            }

            return View(ticket);
        }

        // POST: Tickets/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Tickets == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Tickets'  is null.");
            }

            int companyId = User.Identity!.GetCompanyId();

            Ticket ticket = await _ticketService.GetTicketByIdAsync(id, companyId);

            //var ticket = await _context.Tickets.FindAsync(id);
            if (ticket != null)
            {
                ticket.Archived = true;
                await _ticketService.UpdateTicketAsync(ticket);
            }
            
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
