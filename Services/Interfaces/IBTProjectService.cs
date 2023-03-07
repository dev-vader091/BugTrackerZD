using BugHunterBugTrackerZD.Models;

namespace BugHunterBugTrackerZD.Services.Interfaces
{
    public interface IBTProjectService
    {
        public Task AddProjectAsync(Project project);
        public Task UpdateProjectAsync(Project project);

        public Task<IEnumerable<Project>> GetProjectsAsync(int companyId);

        public Task<Project> GetProjectAsync(int? id, int companyId);
        public Task DeleteProjectAsync(Project project);

       public Task<IEnumerable<Ticket>> GetProjectTicketsAsync(int companyId);

        public Task AddMemberToProjectAsync(IEnumerable<string> memberIds, int projectId);
    }
}
