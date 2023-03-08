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

       public Task<IEnumerable<Ticket>> GetProjectTicketsAsync(int? id);

        public Task AddMembersToProjectAsync(IEnumerable<string> userIds, int? projectId, int? companyId);

        public Task RemoveMembersFromProjectAsync(int? projectId, int? companyId);

        public Task<bool> AddMemberToProjectAsync(BTUser? member, int? projectId);

        public Task<bool> AddProjectManagerAsync(string? userId, int? projectId);

        public Task<Project> GetProjectByIdAsync(int? projectId, int? companyId);

        public Task<BTUser> GetProjectManagerAsync(int? projectId);
        
        public Task RemoveProjectManagerAsync(int? projectId);

        public Task<bool> RemoveMemberFromProjectAsync(BTUser? member, int? projectId);
    }
}
