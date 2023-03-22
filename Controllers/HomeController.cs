using BugHunterBugTrackerZD.Extensions;
using BugHunterBugTrackerZD.Models;
using BugHunterBugTrackerZD.Models.ChartModels;
using BugHunterBugTrackerZD.Models.Enums;
using BugHunterBugTrackerZD.Models.ViewModels;
using BugHunterBugTrackerZD.Services;
using BugHunterBugTrackerZD.Services.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Data;
using System.Diagnostics;

namespace BugHunterBugTrackerZD.Controllers
{
    
    public class HomeController : Controller
    {
        private readonly ILogger<HomeController> _logger;
        private readonly IBTProjectService _projectService;
        private readonly IBTCompanyService _companyService;
        private readonly IBTTicketService _ticketService;


        public HomeController(ILogger<HomeController> logger, IBTProjectService projectService, IBTCompanyService companyService, IBTTicketService ticketService)
        {
            _logger = logger;
            _projectService = projectService;
            _companyService = companyService;
            _ticketService = ticketService;
        }

        [HttpPost]
        public async Task<JsonResult> AmCharts()
        {

            AmChartData amChartData = new();
            List<AmItem> amItems = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = (await _projectService.GetAllProjectsByCompanyAsync(companyId)).Where(p => p.Archived == false).ToList();

            foreach (Project project in projects)
            {
                AmItem item = new();

                item.Project = project.Name;
                item.Tickets = project.Tickets.Count;
                item.Developers = (await _projectService.GetProjectMembersByRoleAsync(project.Id, nameof(BTRoles.Developer))).Count();

                amItems.Add(item);
            }

            amChartData.Data = amItems.ToArray();


            return Json(amChartData.Data);
        }


        public async Task<IActionResult> Dashboard()
        {
            int companyId = User.Identity!.GetCompanyId();

            DashboardViewModel viewModel = new()
            {
                Company = await _companyService.GetCompanyInfoAsync(companyId),
                Projects = await _projectService.GetAllProjectsByCompanyAsync(companyId),
                Tickets = await _ticketService.GetTicketsByIdAsync(companyId),
                Members = await _companyService.GetMembersAsync(companyId),

            };

            return View(viewModel);
        }

        public IActionResult Index()
        {
            return View();
        }

        public IActionResult Landing() 
        {
            return View();
        }

        [HttpPost]
        public async Task<JsonResult> GglProjectTickets()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "ProjectName", "TicketCount" });

            foreach (Project prj in projects)
            {
                chartData.Add(new object[] { prj.Name!, prj.Tickets.Count() });
            }

            return Json(chartData);
        }


        [HttpPost]
        public async Task<JsonResult> GglProjectPriority()
        {
            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

            List<object> chartData = new();
            chartData.Add(new object[] { "Priority", "Count" });


            foreach (string priority in Enum.GetNames(typeof(BTProjectPriorities)))
            {
                int priorityCount = (await _projectService.GetAllProjectsByPriorityAsync(companyId, priority)).Count();
                chartData.Add(new object[] { priority, priorityCount });
            }

            return Json(chartData);
        }


        [HttpPost]
        public async Task<JsonResult> PlotlyBarChart()
        {
            PlotlyBarData plotlyData = new();
            List<PlotlyBar> barData = new();

            int companyId = User.Identity!.GetCompanyId();

            List<Project> projects = await _projectService.GetAllProjectsByCompanyAsync(companyId);

            //Bar One
            PlotlyBar barOne = new()
            {
                X = projects.Select(p => p.Name).ToArray()!,
                Y = projects.SelectMany(p => p.Tickets).GroupBy(t => t.ProjectId).Select(g => g.Count()).ToArray(),
                Name = "Tickets",
                Type = "bar"
            };

            //Bar Two
            PlotlyBar barTwo = new()
            {
                X = projects.Select(p => p.Name).ToArray()!,
                Y = projects.Select(async p => (await _projectService.GetProjectMembersByRoleAsync(p.Id, nameof(BTRoles.Developer))).Count).Select(c => c.Result).ToArray(),
                Name = "Developers",
                Type = "bar"
            };

            barData.Add(barOne);
            barData.Add(barTwo);

            plotlyData.Data = barData;

            return Json(plotlyData);
        }

        public IActionResult Privacy()
        {
            return View();
        }

        public IActionResult Porto()
        {
            return View();
        }

        public IActionResult SignIn()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}