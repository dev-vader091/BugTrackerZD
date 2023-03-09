using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using Microsoft.EntityFrameworkCore;
using BugHunterBugTrackerZD.Data;
using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Models.ViewModels;
using BugHunterBugTrackerZD.Extensions;
using BugHunterBugTrackerZD.Services.Interfaces;
using BugHunterBugTrackerZD.Models.Enums;

namespace BugHunterBugTrackerZD.Controllers
{
    public class CompaniesController : Controller
    {
        private readonly ApplicationDbContext _context;
        private readonly IBTCompanyService _companyService;
        private readonly IBTRolesService _rolesService;

        public CompaniesController(ApplicationDbContext context, IBTCompanyService companyService, IBTRolesService rolesService)
        {
            _context = context;
            _companyService = companyService;
            _rolesService = rolesService;
        }

        // GET: Companies
        public async Task<IActionResult> Index()
        {
              return _context.Companies != null ? 
                          View(await _context.Companies.ToListAsync()) :
                          Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
        }

        // GET: Companies/Details/5
        public async Task<IActionResult> Details(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // GET: Manage User Roles
        [HttpGet]
        public async Task<IActionResult> ManageUserRoles()
        {
            // 1. - Add an instance of the ViewModel as a list (model)
            List<ManageUserRolesViewModel> model = new();

            // 2. - Get companyId
            int companyId = User.Identity!.GetCompanyId();

            // 3. - Get all company users 
            List<BTUser> companyUsers = await _companyService.GetMembersAsync(companyId);

            // 4. - Loop over the users to populate an instance of the ViewModel 
            //         - instantiate single ViewModel 
            //         - use _rolesService to help populate the viewmodel instance 
            //         - instantiate and assign the multiselect 
            //         - add the viewmodel to model list



            foreach (BTUser companyUser in companyUsers)
            {
               
                List<string> currentRoles = (await _rolesService.GetUserRolesAsync(companyUser)).ToList();

                ManageUserRolesViewModel viewModel = new()
                {
                    BTUser = companyUser,
                    Roles = new MultiSelectList(await _rolesService.GetRolesAsync(), "Id", "Name", currentRoles)
                    
                };

                model.Add(viewModel);
            }
            // 5. - Return the model to the View 
            return View(model);

        }

        // POST: Manage User Roles
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ManageUserRoles(ManageUserRolesViewModel viewModel)
        {
            // 1. - Get the companyId
            int companyId = User.Identity!.GetCompanyId();

            // 2. - Instantiate the BTUser 
            BTUser? btUser = viewModel.BTUser;

            // 3. - Get the Roles for the User
            List<string> currentRoles = (await _rolesService.GetUserRolesAsync(btUser!)).ToList();

            // 4. - Get selected Roles for the User submitted from the form
            if (viewModel.SelectedRoles != null)
            {
                List<string>? selectedRoles = viewModel.SelectedRoles;

                // 5. - Remove current Role(s) and Add new role
                await _rolesService.RemoveUserFromRolesAsync(btUser!, currentRoles);

                foreach (string role in selectedRoles)
                {
                    await _rolesService.AddUserToRoleAsync(btUser!, role);
                }
            
            }

            // 6. - Navigate 
            return RedirectToAction(nameof(Details), new { id = viewModel.BTUser!.Id });

        }

        // GET: Companies/Create
        public IActionResult Create()
        {
            return View();
        }

        // POST: Companies/Create
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (ModelState.IsValid)
            {
                _context.Add(company);
                await _context.SaveChangesAsync();
                return RedirectToAction(nameof(Index));
            }
            return View(company);
        }

        // GET: Companies/Edit/5
        public async Task<IActionResult> Edit(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies.FindAsync(id);
            if (company == null)
            {
                return NotFound();
            }
            return View(company);
        }

        // POST: Companies/Edit/5
        // To protect from overposting attacks, enable the specific properties you want to bind to.
        // For more details, see http://go.microsoft.com/fwlink/?LinkId=317598.
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit(int id, [Bind("Id,Name,Description,ImageFileData,ImageFileType")] Company company)
        {
            if (id != company.Id)
            {
                return NotFound();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    _context.Update(company);
                    await _context.SaveChangesAsync();
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!CompanyExists(company.Id))
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
            return View(company);
        }

        // GET: Companies/Delete/5
        public async Task<IActionResult> Delete(int? id)
        {
            if (id == null || _context.Companies == null)
            {
                return NotFound();
            }

            var company = await _context.Companies
                .FirstOrDefaultAsync(m => m.Id == id);
            if (company == null)
            {
                return NotFound();
            }

            return View(company);
        }

        // POST: Companies/Delete/5
        [HttpPost, ActionName("Delete")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteConfirmed(int id)
        {
            if (_context.Companies == null)
            {
                return Problem("Entity set 'ApplicationDbContext.Companies'  is null.");
            }
            var company = await _context.Companies.FindAsync(id);
            if (company != null)
            {
                _context.Companies.Remove(company);
            }
            
            await _context.SaveChangesAsync();
            return RedirectToAction(nameof(Index));
        }

        private bool CompanyExists(int id)
        {
          return (_context.Companies?.Any(e => e.Id == id)).GetValueOrDefault();
        }
    }
}
