using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugHunterBugTrackerZD.Services
{
    public class BTProjectService : IBTProjectService
    {
        private readonly ApplicationDbContext _context;


        public BTProjectService(ApplicationDbContext context)
        {
            _context = context;
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

        public async Task<Project> GetProjectAsync(int? id)
        {
            try
            {
                Project? project = await _context.Projects
                                                 .Include(p => p.Company)
                                                 .Include(p => p.Tickets)
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
    }

}
