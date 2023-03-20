using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Models.Enums;
using BugHunterBugTrackerZD.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugHunterBugTrackerZD.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;
        private readonly IBTRolesService _roleService;

        public BTProjectService(ApplicationDbContext context,
                                UserManager<BTUser> userManager, 
                                IBTRolesService rolesService)
        {
            _context = context;
            _userManager = userManager;
            _roleService = rolesService;
        }
        public async Task AddProjectAsync(Project project)
        {
            try
            {
                _context.Add(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task UpdateProjectAsync(Project project)
        {
            try
            {
                _context.Update(project);
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<IEnumerable<Project>> GetProjectsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projects = new List<Project>();

                projects = await _context.Projects
                                     .Where(p => p.Archived == false && p.CompanyId == companyId)
                                     .Include(p => p.Company)
                                     .Include(p => p.Members)
                                     .Include(p => p.ProjectPriority)
                                     .Include(p => p.Tickets)
                                     .ToListAsync();

                return projects;

            }
            catch (Exception)
            {

                throw;
            }

        }

        public async Task<IEnumerable<Project>> GetArchivedProjectsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projects = new List<Project>();

                projects = await _context.Projects
                                     .Where(p => p.Archived == true && p.CompanyId == companyId)
                                     .Include(p => p.Company)
                                     .Include(p => p.Members)
                                     .Include(p => p.ProjectPriority)
                                     .Include(p => p.Tickets)
                                     .ToListAsync();

                return projects;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project> GetProjectAsync(int? id, int companyId)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Where(p => p.CompanyId == companyId)
                                                 .Include(p => p.Company)
                                                 .Include(p => p.Members)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                   
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.SubmitterUser)
                                                    
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                                    .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                                 .Include(p => p.ProjectPriority)
                                                 .FirstOrDefaultAsync(p => p.Id == id);
                                                 
                                                 

                return project!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        
        public Task DeleteProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Ticket>> GetProjectTicketsAsync(int? id)
        {
            try
            {
                IEnumerable<Ticket> tickets = await _context.Tickets
                                                        .Where(t => t.ProjectId == id)
                                                        .Include(t => t.Project)
                                                        .Include(t => t.SubmitterUser)
                                                        .Include(t => t.DeveloperUser)
                                                        .Include(t => t.Comments)
                                                        .ToListAsync();

                return tickets;
            }
            catch (Exception)
            {

                throw;
            }
        }


        public async Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member.Id);

                if (!IsOnProject)
                {
                    project.Members.Add(member);
                    await _context.SaveChangesAsync();
                    return true; 
                }
                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> AddProjectManagerAsync(string? userId, int? projectId)
        {
            try
            {
                BTUser? currentPM = await GetProjectManagerAsync(projectId);
                BTUser? selectedPM = await _context.Users.FindAsync(userId);

                // Removed current Project Manager 
                if (selectedPM != null) 
                {
                    await RemoveProjectManagerAsync(projectId);
                }
                // Add selected Project Manager
                try
                {
                    await AddMemberToProjectAsync(selectedPM, projectId);
                    return true;
                }
                catch (Exception)
                {

                    throw;
                }

            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<Project> GetProjectByIdAsync(int? projectId, int? companyId)
        {
            Project? project = await _context.Projects
                                                 .Where(p => p.CompanyId == companyId)
                                                 .Include(p => p.Company)
                                                 .Include(p => p.Members)
                                                 .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.DeveloperUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.SubmitterUser)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketPriority)
                                                    .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketStatus)
                                                .Include(p => p.Tickets)
                                                    .ThenInclude(t => t.TicketType)
                                                 .Include(p => p.ProjectPriority)
                                                 .FirstOrDefaultAsync(p => p.Id == projectId);

            return project!;
        }

        public async Task<BTUser> GetProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await  _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        return member;
                    }
                }

                return null!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveProjectManagerAsync(int? projectId)
        {
            try
            {
                Project? project = await _context.Projects.Include(p => p.Members).FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (BTUser member in project!.Members)
                {
                    if (await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        await RemoveMemberFromProjectAsync(member, projectId);
                    }
                }
               
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, member!.CompanyId);

                bool IsOnProject = project.Members.Any(m => m.Id == member!.Id);

                if (IsOnProject)
                {
                    project.Members.Remove(member);
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                foreach (string userId in userIds)
                {
                    BTUser? btUser = await _context.Users.FindAsync(userId);

                    if (project != null && btUser != null) 
                    {
                        bool IsOnProject = project.Members.Any(m => m.Id == userId);

                        if (!IsOnProject)
                        {
                            project.Members.Add(btUser);
                        }
                        else
                        {
                            continue;
                        }
                    }
                }
                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task RemoveMembersFromProjectAsync(int? projectId, int? companyId)
        {
            try
            {
                Project? project = await GetProjectByIdAsync(projectId, companyId);

                foreach (BTUser member in project.Members)
                {
                    if (!await _roleService.IsUserInRoleAsync(member, nameof(BTRoles.ProjectManager)))
                    {
                        project.Members.Remove(member);
                    }   
                }

                await _context.SaveChangesAsync();
            }
            catch (Exception)
            {

                throw;
            }
        }

       
    }

}
