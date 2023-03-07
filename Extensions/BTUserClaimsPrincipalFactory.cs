﻿using BugHunterBugTrackerZD.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using System.Security.Claims;

namespace BugHunterBugTrackerZD.Extensions
{
    public class BTUserClaimsPrincipalFactory : UserClaimsPrincipalFactory<BTUser, IdentityRole>
    {
        public BTUserClaimsPrincipalFactory(UserManager<BTUser> userManager,
                                            RoleManager<IdentityRole> roleManager, 
                                            IOptions<IdentityOptions> optionsAccessor)
             :base(userManager, roleManager, optionsAccessor)
        {

        }

        protected override async Task<ClaimsIdentity> GenerateClaimsAsync(BTUser user)
        {
            ClaimsIdentity identity = await base.GenerateClaimsAsync(user);
            identity.AddClaim(new Claim("CompanyId", user.CompanyId.ToString()));
            return identity;
        }
    }

}

// override = there's inheritance that needs to be changed
