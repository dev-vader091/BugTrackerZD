using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Models.Enums;
using BugHunterBugTrackerZD.Services.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace BugHunterBugTrackerZD.Services
{
    public class BTCompanyService : IBTCompanyService
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTRolesService _rolesService;
        public BTCompanyService(ApplicationDbContext context, IBTRolesService rolesService)
        {
            _context = context;
            _rolesService = rolesService;
        }

        public async Task<BTUser> GetCompanyAdmin(int? companyId, string? userId)
        {
           List<BTUser> members = new List<BTUser>();

            BTUser? admin = new();

            members = await GetMembersAsync(companyId);

            foreach (BTUser member in members) 
            {
                if (await _rolesService.IsUserInRoleAsync(member, nameof(BTRoles.Admin)) && member.Id == userId)
                {
                    admin = member;
                }
            }

            return admin;
            
        }

        public async Task<Company> GetCompanyInfoAsync(int? companyId)
        {
            try
            {
                Company? company = new();

                if(companyId != null) 
                {
                
                company = await _context.Companies
                                                 .Include(c => c.Projects)
                                                 .Include(c => c.Members)
                                                 .Include(c => c.Invites)
                                                 .FirstOrDefaultAsync(c => c.Id == companyId);
                }
                return company!;
            }
            catch (Exception)
            {

                throw;
            }
        }

        public async Task<List<BTUser>> GetMembersAsync(int? companyId)
        {
            try
            {
                List<BTUser> members = new();

                members = await _context.Users
                                        .Where(u => u.CompanyId == companyId)                                   
                                        .ToListAsync();
                return members;
            }
            catch (Exception)
            {

                throw;
            }
        }


    }
}
