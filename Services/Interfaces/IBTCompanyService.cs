using BugHunterBugTrackerZD.Models;

namespace BugHunterBugTrackerZD.Services.Interfaces
{
    public interface IBTCompanyService
    {
        public Task<Company> GetCompanyInfoAsync(int? companyId);

        public Task<List<BTUser>> GetMembersAsync(int? companyId);

        public Task<BTUser> GetCompanyAdmin(int? companyId, string? userId);
    }
}
