using BugHunterBugTrackerZD.Models;

namespace BugHunterBugTrackerZD.Services.Interfaces
{
    public interface IBTProjectService
    {
        public Task AddProjectAsync(Project project);
        public Task UpdateProjectAsync(Project project);

        public Task<Project> GetProjectAsync(int? id);
        public Task DeleteProjectAsync(Project project);
    }
}
