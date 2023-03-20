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
using System.IO;

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
        private readonly IBTFileService _fileService;
        private readonly IBTProjectService _projectService;
        private readonly IBTNotificationService _notificationService;
        private readonly IBTCompanyService _companyService;

        public TicketsController(ApplicationDbContext context,
                                 UserManager<BTUser> userManager,
                                 IBTTicketService ticketService,
                                 IBTRolesService rolesService,
                                 IBTTicketHistoryService historyService,
                                 IBTFileService fileService,
                                 IBTProjectService projectService,
                                 IBTNotificationService notificationService,
                                 IBTCompanyService companyService)
        {
            _context = context;
            _userManager = userManager;
            _ticketService = ticketService;
            _rolesService = rolesService;
            _historyService = historyService;
            _fileService = fileService;
            _projectService = projectService;
            _notificationService = notificationService;
            _companyService = companyService;
        }



        // POST: Add Ticket Attachment 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddTicketAttachment([Bind("Id,FormFile,Description,TicketId")] TicketAttachment ticketAttachment)
        {
            string statusMessage;

            ModelState.Remove("BTUserId");
            if (ModelState.IsValid && ticketAttachment.FormFile != null)
            {
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                Ticket? ticket = await _context.Tickets
                                               .Include(t => t.Attachments)
                                               .Include(t => t.DeveloperUserId)
                                               .Include(t => t.SubmitterUserId)
                                               .FirstOrDefaultAsync(t => t.Id == ticketAttachment.TicketId);

                //if (ticket!.DeveloperUserId == ticketAttachment.BTUserId || ticket.SubmitterUserId == ticketAttachment.BTUserId)
                //{

                //}

                ticketAttachment.FileData = await _fileService.ConvertFileToByteArrayAsync(ticketAttachment.FormFile);
                ticketAttachment.FileName = ticketAttachment.FormFile.FileName;
                ticketAttachment.FileType = ticketAttachment.FormFile.ContentType;

                ticketAttachment.Created = DataUtility.GetPostGresDate(DateTime.Now);
                ticketAttachment.BTUserId = _userManager.GetUserId(User);

                await _ticketService.AddTicketAttachmentAsync(ticketAttachment);
                statusMessage = "Success: New attachment added to Ticket.";
            }
            else
            {
                statusMessage = "Error: Invalid data.";

            }

            return RedirectToAction("Details", new { id = ticketAttachment.TicketId, message = statusMessage });
        }

        public async Task<IActionResult> ShowFile(int id)
        {
            TicketAttachment ticketAttachment = await _ticketService.GetTicketAttachmentByIdAsync(id);
            string fileName = ticketAttachment.FileName!;
            byte[] fileData = ticketAttachment.FileData!;
            string ext = Path.GetExtension(fileName)!.Replace(".", "");

            Response.Headers.Add("Content-Disposition", $"inline; filename={fileName}");
            return File(fileData, $"application/{ext}");
        }

        // POST: Add Ticket Comment 
        public async Task<IActionResult> AddTicketComment([Bind("Id,Comment,Created,TicketId,UserId")] TicketComment ticketComment)
        {
            ModelState.Remove("UserId");
            if (ModelState.IsValid)
            {
                BTUser? btUser = await _userManager.GetUserAsync(User);   
                string? userId = _userManager.GetUserId(User);
                ticketComment.UserId = userId;

                //if (ticketComment.Ticket!.DeveloperUserId == userId || ticketComment.Ticket.SubmitterUserId == userId || ticketComment.Ticket.Project!.Members.Contains(btUser!))
                //{

                //}

                ticketComment.Created = DataUtility.GetPostGresDate(DateTime.Now);


                _context.Add(ticketComment);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));

            }
            //ViewData["TicketId"] = new SelectList(_context.Tickets, "Id", "Description", ticketComment.TicketId);
            //ViewData["UserId"] = new SelectList(_context.Users, "Id", "Id", ticketComment.UserId);
            
            return View(ticketComment);

           
        }

        // GET: Archived Tickets 
        public async Task<IActionResult> ArchivedTickets()
        {
            int companyId = User.Identity!.GetCompanyId();
            
            IEnumerable<Ticket> archivedTickets = await _ticketService.GetArchivedTicketsAsync(companyId);


            return View(archivedTickets);
        }
        
        // GET: Assign Developer 
        [HttpGet]
        public async Task<IActionResult> AssignDeveloper(int? id)
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
                if (await _rolesService.IsUserInRoleAsync(user, nameof(BTRoles.Developer)) || await _rolesService.IsUserInRoleAsync(user, nameof(BTRoles.Submitter)))
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

        // POST: Assign Developer 
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AssignDeveloper(TicketUsersViewModel viewModel)
        {
            string? userId = _userManager.GetUserId(User);
            int companyId = User.Identity!.GetCompanyId();

            Ticket? oldticket = await _ticketService.GetTicketByIdAsync(viewModel.Ticket!.Id, companyId);

            if (!string.IsNullOrEmpty(viewModel.SelectedDeveloperId))
            {

                // Add developerId to ticket              
                await _ticketService.AssignTicketToDeveloperAsync(viewModel.Ticket.Id, viewModel.SelectedDeveloperId, companyId);


                // TODO: Add Ticket History
                await _ticketService.UpdateTicketAsync(oldticket);

                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(oldticket.Id, companyId);

                await _historyService.AddHistoryAsync(oldticket, newTicket, userId);

                // TODO: Add Ticket Notification

                return RedirectToAction("Details", new { id = viewModel.Ticket?.Id });
            }

            //// TODO: Add Ticket History
            //await _ticketService.UpdateTicketAsync(oldticket);

            //Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(oldticket.Id, companyId);

            //await _historyService.AddHistoryAsync(oldticket, newTicket, userId);

            //// TODO: Add Ticket Notification

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
            string? userId = _userManager.GetUserId(User);
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Ticket> tickets = new List<Ticket>();

            tickets = await _ticketService.GetTicketsByIdAsync(companyId);

            return View(tickets);
            
        }

        // GET: My Tickets
        public async Task<IActionResult> MyTickets()
        {           
            string? userId = _userManager.GetUserId(User);

            BTUser? btUser = await _context.Users
                                           .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.TicketPriority)
                                           .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.TicketStatus)
                                           .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.TicketType)
                                           .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.DeveloperUser)
                                           .Include(u => u.Projects)
                                            .ThenInclude(p => p.Tickets)
                                            .ThenInclude(t => t.SubmitterUser)
                                           .FirstOrDefaultAsync(u => u.Id == userId);

            List<Ticket> tickets = new List<Ticket>();

            tickets = btUser!.Projects.SelectMany(p => p.Tickets).Where(t => t.DeveloperUserId == userId || t.SubmitterUserId == userId).ToList();

            
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

                // TODO: Set ticket status through context 


                

                await _ticketService.AddTicketAsync(ticket);

                

                int companyId = User.Identity!.GetCompanyId();
                Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                await _historyService.AddHistoryAsync(null, newTicket, user.Id);

                // Notification
                BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);

                Notification? notification = new()
                {
                    TicketId = ticket.Id,
                    ProjectId = ticket.ProjectId,
                    Title = "New Ticket Added",
                    Message = $"New Ticket: {ticket.Title} was created by: {user.FullName}. ",
                    Created = DataUtility.GetPostGresDate(DateTime.Now),
                    SenderId = user.Id,
                    RecipientId = projectManager.Id,
                    NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                };

                if (projectManager != null)
                {
                    await _notificationService.AddNotificationAsync(notification);
                    await _notificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                }
                else
                {
                    await _notificationService.AdminNotificationAsync(notification, companyId);
                    await _notificationService.SendAdminEmailNotificationAsync(notification, "New Project Ticket Added", companyId);
                }
                return RedirectToAction(nameof(AssignDeveloper), new {id = ticket.Id});
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
        //[Authorize(Roles = "ProjectManager, Developer")]
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
            //ViewData["DeveloperUserId"] = new SelectList(_context.Users, "Id", "Id", ticket.DeveloperUserId);
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
        //[Authorize(Roles = "ProjectManager, Developer" )]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Title,Description,Created,Updated,Archived,ArchivedByProject,ProjectId,TicketTypeId,TicketStatusId,TicketPriorityId,DeveloperUserId,SubmitterUserId")] Ticket ticket)
        {
            if (id != ticket.Id)
            {
                return NotFound();
            }

            
            if (ModelState.IsValid)
            {
                BTUser? user = await _userManager.GetUserAsync(User);
                try
                {
                    
                    int companyId = User.Identity!.GetCompanyId();

                    Ticket? oldTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                    // Reformat Created/Updated Dates
                    ticket.Created = DataUtility.GetPostGresDate(ticket.Created);
                    ticket.Updated = DataUtility.GetPostGresDate(DateTime.UtcNow);

                   
                    await _ticketService.UpdateTicketAsync(ticket);

                    Ticket? newTicket = await _ticketService.GetTicketAsNoTrackingAsync(ticket.Id, companyId);

                    await _historyService.AddHistoryAsync(oldTicket, newTicket, user!.Id);

                    // Notification
                    BTUser? projectManager = await _projectService.GetProjectManagerAsync(ticket.ProjectId);

                    Notification? notification = new()
                    {
                        TicketId = ticket.Id,
                        ProjectId = ticket.ProjectId,
                        Title = "New Ticket Added",
                        Message = $"New Ticket: {ticket.Title} was created by: {user.FullName}. ",
                        Created = DataUtility.GetPostGresDate(DateTime.Now),
                        SenderId = user.Id,
                        RecipientId = projectManager.Id,
                        NotificationTypeId = (await _context.NotificationTypes.FirstOrDefaultAsync(n => n.Name == nameof(BTNotificationTypes.Ticket)))!.Id
                    };

                    if (projectManager != null)
                    {
                        await _notificationService.AddNotificationAsync(notification);
                        await _notificationService.SendEmailNotificationAsync(notification, "New Ticket Added");
                    }
                    else
                    {
                        await _notificationService.AdminNotificationAsync(notification, companyId);
                        await _notificationService.SendAdminEmailNotificationAsync(notification, "New Project Ticket Added", companyId);
                    }

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

            //if (ticket!.Project!.Archived == true)
            //{
            //    ticket.ArchivedByProject = true;
            //    await _ticketService.UpdateTicketAsync(ticket);
            //}
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool TicketExists(int id)
        {
          return (_context.Tickets?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
