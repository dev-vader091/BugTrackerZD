using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Authorization;
using BugHunterBugTrackerZD.Models.Enums;
using BugHunterBugTrackerZD.Services.Interfaces;
using BugHunterBugTrackerZD.Extensions;
using System.ComponentModel.Design;
using BugHunterBugTrackerZD.Models.ViewModels;
using BugHunterBugTrackerZD.Services;

namespace BugHunterBugTrackerZD.Controllers
{
    [Authorize]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;
        private readonly IBTRolesService _rolesService;
        private readonly IBTFileService _fileService;
        private readonly IBTTicketService _ticketService;

        public ProjectsController(ApplicationDbContext context,
                                  UserManager<BTUser> usermanager,
                                  IBTProjectService projectService,
                                  IBTRolesService rolesService,
                                  IBTFileService fileService,
                                  IBTTicketService ticketService)
        {
            _context = context;
            _userManager = usermanager;
            _projectService = projectService;
            _rolesService = rolesService;
            _fileService = fileService;
            _ticketService = ticketService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Project> projects = await _projectService.GetProjectsAsync(companyId);
                                     
            
            return View(projects);


            //var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.ProjectPriority);
            //return View(await applicationDbContext.ToListAsync());
        }

        // GET: Assign Project Members 
        [HttpGet]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(int? id)
        {
           

            if (id == null)
            {
                return NotFound();

            }

            int companyId = User.Identity!.GetCompanyId();

            Project project = await _projectService.GetProjectByIdAsync(id, companyId);

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);

            // Concat adds the two lists together
            List<BTUser> userList = submitters.Concat(developers).ToList();

            List<string> currentMembers = project.Members.Select(m => m.Id).ToList();

            ProjectMembersViewModel viewModel = new()
            {
                Project = project,
                UsersList = new MultiSelectList(userList, "Id", "FullName", currentMembers)
            };

            return View(viewModel);

        }

        // POST: Assign Project Members 
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> AssignProjectMembers(ProjectMembersViewModel viewModel)
        {
            int companyId = User.Identity!.GetCompanyId();

            if (viewModel.SelectedMembers != null)
            {
                // Remove current members 
                await _projectService.RemoveMembersFromProjectAsync(viewModel.Project!.Id, companyId);

                // Add newly selected members 
                await _projectService.AddMembersToProjectAsync(viewModel.SelectedMembers, viewModel.Project!.Id, companyId);

                return RedirectToAction(nameof(Details), new { id  = viewModel.Project.Id });
            }

            ModelState.AddModelError("SelectedMembers", "No members selected. Please select users.");
            // Reset the form
            viewModel.Project = await _projectService.GetProjectByIdAsync(viewModel.Project!.Id, companyId);
            List<string> currentMembers = viewModel.Project.Members.Select(m => m.Id).ToList();

            List<BTUser> submitters = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Submitter), companyId);
            List<BTUser> developers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.Developer), companyId);
            List<BTUser> userList = submitters.Concat(developers).ToList();
            viewModel.UsersList = new MultiSelectList(userList, "Id", "FullName", currentMembers);

            return View(viewModel);
        }

        // GET: AssignPM
        [HttpGet]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(int? id)
        {
            // 1. Validate id
            // 2. Get companyId
            // 3.Get & Assign Project property of the view Model
            // 4. Find current PM if one is assigned 
            // 5. Create SelectList of company's PMs (highlight the current PM if one is assigned)
            // 6. Create/Instantiate PMViewModel
            // 7. Return View() using PMViewModel as the model 

            if (id == null)
            {
                return NotFound();

            }

            // Get CompanyId
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(id);
            AssignPMViewModel viewModel = new()
            {
                Project = await _projectService.GetProjectByIdAsync(id, companyId),
                PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id),
                PMId = currentPM?.Id
            };

            return View(viewModel);


        }

        // POST: AssignPM
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin")]
        public async Task<IActionResult> AssignPM(AssignPMViewModel viewModel)
        {
            if (!string.IsNullOrEmpty(viewModel.PMId))
            {
                await _projectService.AddProjectManagerAsync(viewModel.PMId, viewModel.Project?.Id);

                return RedirectToAction("Details", new { id = viewModel.Project?.Id });
            
            }

            ModelState.AddModelError("PMId", "No Project Manager chosen. Please select a PM.");
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<BTUser> projectManagers = await _rolesService.GetUsersInRoleAsync(nameof(BTRoles.ProjectManager), companyId);
            BTUser? currentPM = await _projectService.GetProjectManagerAsync(viewModel.Project?.Id);
            viewModel.Project = await _projectService.GetProjectByIdAsync(viewModel.Project?.Id, companyId);
            viewModel.PMList = new SelectList(projectManagers, "Id", "FullName", currentPM?.Id);
            viewModel.PMId = currentPM?.Id;

            return View(viewModel);
        }

        // GET: My Projects 
        public async Task<IActionResult> MyProjects()
        {
            //BTUser? user = await _userManager.GetUserAsync(User);

            string? userId = _userManager.GetUserId(User);

            BTUser? user = await _context.Users   
                                  .Include(u => u.Projects)
                                    .ThenInclude(p => p.ProjectPriority)
                                .Include(u => u.Company)
                                .Include(u => u.Projects)
                                    .ThenInclude(p => p.Members)
                                  .FirstOrDefaultAsync(u => u.Id == userId); 

            int companyId = User.Identity!.GetCompanyId();

            //List<Project> projects = new List<Project>();

            List<Project> myProjects = new List<Project>();

            myProjects = user!.Projects.ToList();

            
            return View(myProjects);
        }

        // GET: Archived Projects
        public async Task<IActionResult> ArchivedProjects(int? id)
        {
            

            int companyId = User.Identity!.GetCompanyId();

            


            IEnumerable<Project> archivedProjects = await _projectService.GetArchivedProjectsAsync(companyId);

            return View(archivedProjects);
           
        }

        // GET Unarchive Project
        public async Task<IActionResult> UnarchiveProject(int? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            int companyId = User.Identity!.GetCompanyId();

            Project project = await _projectService.GetProjectByIdAsync(id, companyId);


            if (project == null)
            {
                return NotFound();
            }


            return View(project);
        }

        // POST: Unarchive Project
        [HttpPost, ActionName("UnarchiveProject")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UnarchiveProjectConfirmed(int id)
        {

            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

            int companyId = User.Identity!.GetCompanyId();


            Project project = await _projectService.GetProjectAsync(id, companyId);
            List<Ticket> tickets = await _ticketService.GetTicketsByProjectAsync(project.Id);

            foreach (Ticket ticket in tickets)
            {
                ticket.Archived = true;
                ticket.ArchivedByProject = false;
                await _ticketService.UpdateTicketAsync(ticket);
            }

            //var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.Archived = false;
                await _projectService.UpdateProjectAsync(project);
            }

            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }



        // GET: Unassigned Projects 
        public async Task<IActionResult> UnassignedProjects()
        {
            int companyId = User.Identity!.GetCompanyId();

            IEnumerable<Project> projects = await _projectService.GetProjectsAsync(companyId);

            List<Project> unassignedProjects = new List<Project>();

            foreach (Project project in projects)
            {
                BTUser projectManager = await _projectService.GetProjectManagerAsync(companyId);
               
                    if (!project.Members.Any(m => m.Id == projectManager.Id) || project.Members.Count == 0)
                    {
                        unassignedProjects.Add(project);
                    }
                
            }

            return View(unassignedProjects);  
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id, int companyId)
        {
            if (id == null)
            {
                return NotFound();
            }
            companyId = User.Identity!.GetCompanyId();

            Project project = await _projectService.GetProjectAsync(id, companyId);                          

            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create()
        {
            BTUser? user = await _userManager.GetUserAsync(User);

            //Project project = new Project();

            //project.CompanyId = user!.CompanyId;

            IEnumerable<BTUser> members = await _context.Users
                                      .Where(m => m.CompanyId == user!.CompanyId)
                                      .ToListAsync();

           


            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Admin, ProjectManager")]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,Archived")] Project project, IEnumerable<string> selected)
        {
            if (ModelState.IsValid)
            {
                
                BTUser? user = await _userManager.GetUserAsync(User);

                project.CompanyId = user!.CompanyId;

                // Format Date(s)
                project.Created = DataUtility.GetPostGresDate(DateTime.Now);

                // TODO: Image Service 
                if (project.ImageFormFile!= null)
                {
                    project.ImageFileData = await _fileService.ConvertFileToByteArrayAsync(project.ImageFormFile);
                    project.ImageFileType = project.ImageFormFile.ContentType;
                }

                if (project.StartDate != null)
                {
                    project.StartDate = DataUtility.GetPostGresDate(project.StartDate.Value);
                }

                if (project.EndDate != null)
                {
                    project.EndDate = DataUtility.GetPostGresDate(project.EndDate.Value);
                }
                
                await _projectService.AddProjectAsync(project);

                // TODO: add members to project
                //await _projectService.AddMemberToProjectAsync(selected, project.Id);
                
                return RedirectToAction(nameof(Index));
            }

            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id, int companyId)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            companyId = User.Identity!.GetCompanyId();


            Project project = await _projectService.GetProjectAsync(id, companyId);

            if (project == null)
            {
                return NotFound();
            }
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // POST: Projects/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,Archived")] Project project)
        {
            if (id != project.Id)
            {
                return NotFound();
            }
            
            if (ModelState.IsValid)
            {
                try
                {
                    BTUser? user = await _userManager.GetUserAsync(User);
                    project.CompanyId = user!.CompanyId;
                    // Reformat Created Date
                    project.Created = DataUtility.GetPostGresDate(project.Created);

                    // Reformat Start Date
                    if (project.StartDate != null)
                    {
                        project.StartDate = DataUtility.GetPostGresDate(project.StartDate.Value);
                    }
                    // Reformat End Date 
                    if (project.EndDate != null)
                    {
                        project.EndDate = DataUtility.GetPostGresDate(project.EndDate.Value);
                    }

                    await _projectService.UpdateProjectAsync(project);
                  
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!ProjectExists(project.Id))
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
            //ViewData["CompanyId"] = new SelectList(_context.Companies, "Id", "Name", project.CompanyId);
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Delete/5
        public async Task<IActionResult> Delete(int? id, int companyId)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            companyId = User.Identity!.GetCompanyId();


            Project project = await _projectService.GetProjectAsync(id, companyId);
           
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id, int companyId)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

            companyId = User.Identity!.GetCompanyId();


            Project project = await _projectService.GetProjectAsync(id, companyId);
            List<Ticket> tickets = await _context.Tickets.Where(t => t.ProjectId == project.Id)
                                                         .Include(t => t.Project)
                                                         .Include(t => t.DeveloperUser)
                                                         .Include(t => t.SubmitterUser)
                                                         .ToListAsync();
            foreach (Ticket ticket in tickets)
            {
                ticket.Archived = true;
                ticket.ArchivedByProject = true;
                await _ticketService.UpdateTicketAsync(ticket);
            }

            //var project = await _context.Projects.FindAsync(id);
            if (project != null)
            {
                project.Archived = true;
                await _projectService.UpdateProjectAsync(project);
            }
            
            //await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool ProjectExists(int id)
        {
          return (_context.Projects?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
