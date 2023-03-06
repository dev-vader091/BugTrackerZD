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

namespace BugHunterBugTrackerZD.Controllers
{
    [Authorize(Roles = "Admin, ProjectManager")]
    public class ProjectsController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTProjectService _projectService;

        public ProjectsController(ApplicationDbContext context, UserManager<BTUser> usermanager, IBTProjectService projectService)
        {
            _context = context;
            _userManager = usermanager;
            _projectService = projectService;
        }

        // GET: Projects
        public async Task<IActionResult> Index()
        {
            BTUser? user = await _userManager.GetUserAsync(User);

            List<Project> projects = new List<Project>();

            projects = await _context.Projects
                                     .Where(p => p.CompanyId == user!.CompanyId)
                                     .Include(p => p.Company)
                                     .Include(p => p.Tickets)
                                     .Include(p => p.ProjectPriority)
                                     .ToListAsync();

            return View(projects);


            //var applicationDbContext = _context.Projects.Include(p => p.Company).Include(p => p.ProjectPriority);
            //return View(await applicationDbContext.ToListAsync());
        }

        // GET: My Projects 
        public async Task<IActionResult> MyProjects()
        {
            BTUser? user = await _userManager.GetUserAsync(User);

            List<Project> projects = new List<Project>();

            projects = await _context.Projects
                                     .Where(p => p.CompanyId == user!.CompanyId)
                                     .Include(p => p.Company)                                    
                                     .Include(p => p.Members)
                                     .Include(p => p.Tickets)
                                     .Include(p => p.ProjectPriority)
                                     .ToListAsync();

            return View(projects);
        }


        // GET: Projects/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            var project = await _context.Projects
                .Include(p => p.Company)
                .Include(p => p.ProjectPriority)
                .FirstOrDefaultAsync(m => m.Id == id);
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // GET: Projects/Create
        public IActionResult Create()
        {
            


            
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name");
            return View();
        }

        // POST: Projects/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,CompanyId,Name,Description,Created,StartDate,EndDate,ProjectPriorityId,ImageFileData,ImageFileType,Archived")] Project project)
        {
            if (ModelState.IsValid)
            {
                
                BTUser? user = await _userManager.GetUserAsync(User);



                project.CompanyId = user!.CompanyId;

                // Format Date(s)
                project.Created = DataUtility.GetPostGresDate(DateTime.Now);

                if (project.StartDate != null)
                {
                    project.StartDate = DataUtility.GetPostGresDate(project.StartDate.Value);
                }

                if (project.EndDate != null)
                {
                    project.EndDate = DataUtility.GetPostGresDate(project.EndDate.Value);
                }

                await _projectService.AddProjectAsync(project);
                
                return RedirectToAction(nameof(Index));
            }
            ViewData["ProjectPriorityId"] = new SelectList(_context.ProjectPriorities, "Id", "Name", project.ProjectPriorityId);
            return View(project);
        }

        // GET: Projects/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project project = await _projectService.GetProjectAsync(id);

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
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Projects == null)
            {
                return NotFound();
            }

            Project project = await _projectService.GetProjectAsync(id);
           
            if (project == null)
            {
                return NotFound();
            }

            return View(project);
        }

        // POST: Projects/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Projects == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Projects'  is null.");
            }

            Project project = await _projectService.GetProjectAsync(id);
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
