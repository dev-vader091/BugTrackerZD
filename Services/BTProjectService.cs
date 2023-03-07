using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Services.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace BugHunterBugTrackerZD.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;
        private readonly UserManager<BTUser> _userManager;

        public BTProjectService(ApplicationDbContext context, UserManager<BTUser> userManager)
        {
            _context = context;
            _userManager = userManager;
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

        public async Task<IEnumerable<Project>> GetProjectsAsync(int companyId)
        {
            try
            {
                IEnumerable<Project> projects = new List<Project>();

                projects = await _context.Projects
                                     .Where(p => p.Archived == false && p.CompanyId == companyId)
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
        
        public Task DeleteProjectAsync(Project project)
        {
            throw new NotImplementedException();
        }

        public async Task<IEnumerable<Ticket>> GetProjectTicketsAsync(int id)
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

        public async Task AddMemberToProjectAsync(IEnumerable<string> memberIds, int projectId)
        {
            try
            {
                // Get project from database
                // Include() is pulling in an ICollection of members above,
                // which is a collection of BTUser objects,
                // the hash set is just there so it's never null, just empty.
                Project? project = await _context.Projects
                                                  .Include(p => p.Members)
                                                  .FirstOrDefaultAsync(p => p.Id == projectId);

                foreach (string memberId in memberIds)
                {
                    // Get the member
                    BTUser? member = await _context.Users.FindAsync(memberId);

                    if (project != null && member != null)
                    {
                        project.Members.Add(member);
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
